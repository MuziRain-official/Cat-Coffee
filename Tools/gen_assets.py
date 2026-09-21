#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
阿里云百炼（通义万相）批量生成像素资产。

用法:
  设置环境变量 DASHSCOPE_API_KEY，或直接改脚本里的 KEY
  python gen_assets.py [--assets all]

流程:
  1. 调万相文生图(wanx2.1-t2i-turbo)，prompt 带"纯白背景单物体"指令
  2. 轮询任务完成后下载原图到 Assets/Art/raw/
  3. 调 pixelize.py 做像素化 + 抠背景，输出到 Assets/Art/
"""
import os
import sys
import time
import json
import urllib.request

KEY = os.environ.get("DASHSCOPE_API_KEY", "")
API = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text2image/image-synthesis"

STYLE = "pixel art game sprite, single object isolated on pure white background, no room no scene no people no shadow, centered, retro 16-bit pixel style, warm brown and cream retro palette"

ASSETS = [
    ("coffee_machine", "a vintage espresso coffee machine, chrome and brass accents, lever handle, steam wand"),
    ("counter", "a wooden bar counter table for assembling drinks"),
    ("warmer", "a food warmer counter with glass display shelf"),
    ("coffee_cup", "a cup of latte coffee with milk foam on top, saucer"),
    ("cat", "an orange tabby cat sitting, cute anime style"),
    ("player", "a cute barista girl with apron and cap, chibi anime style"),
    ("table", "a round wooden cafe table"),
    ("chair", "a wooden cafe chair"),
    ("customer", "a cafe customer, chibi anime style character"),
    ("cat_nest", "a round cat bed cushion"),
    ("cat_pad", "a small rectangular cat mat"),
    ("floor", "wooden floor tile texture, seamless tileable pattern"),
    ("wall", "cream wall texture, seamless tileable pattern"),
]

def request_json(url, data, key, headers=None):
    req = urllib.request.Request(url, data=json.dumps(data).encode("utf-8"),
                                 headers={"Content-Type": "application/json",
                                          "Authorization": "Bearer " + key})
    if headers:
        for k, v in headers.items():
            req.add_header(k, v)
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode("utf-8"))

def generate(key, name, desc, size="1024*1024"):
    prompt = f"{STYLE}, {desc}"
    payload = {
        "model": "wanx2.1-t2i-turbo",
        "input": {"prompt": prompt},
        "parameters": {"size": size, "n": 1},
    }
    r = request_json(API, payload, key, {"X-DashScope-Async": "enable"})
    task_id = r["output"]["task_id"]
    # 轮询
    task_url = f"https://dashscope.aliyuncs.com/api/v1/tasks/{task_id}"
    for _ in range(30):
        time.sleep(3)
        tr = request_json(task_url, {}, key)
        status = tr["output"]["task_status"]
        if status == "SUCCEEDED":
            return tr["output"]["results"][0]["url"]
        if status == "FAILED":
            raise RuntimeError("任务失败: " + json.dumps(tr.get("output", {}), ensure_ascii=False))
    raise RuntimeError("任务超时")

def download(url, path):
    urllib.request.urlretrieve(url, path)

def main():
    if not KEY:
        print("请设置环境变量 DASHSCOPE_API_KEY")
        sys.exit(1)
    os.makedirs("Assets/Art/raw", exist_ok=True)
    for i, (name, desc) in enumerate(ASSETS):
        print(f"[{i+1}/{len(ASSETS)}] 生成 {name}...")
        try:
            url = generate(KEY, name, desc)
            raw = f"Assets/Art/raw/{name}.png"
            download(url, raw)
            print(f"  原图已下载 -> {raw}")
            # 像素化+抠背景
            os.system(f'python Tools/pixelize.py "{raw}" "Assets/Art/{name}.png" --size 96 --bg-tolerance 30')
        except Exception as e:
            print(f"  !! {name} 失败: {e}")

if __name__ == "__main__":
    main()
