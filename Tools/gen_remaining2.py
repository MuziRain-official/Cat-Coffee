#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""续跑剩余资产（跳过已生成 counter）。"""
import os, time, json, subprocess
import urllib.request

KEY = "sk-ws-H.PIYMMMP.it9Z.MEUCIHSFhNGreYh2dDctRXlJKxue29DG7frJtbs0T6slbCi5AiEA0JoQxqsu_TUZrnBL62OL_6bjXMvtrbMv2aVuJYKFPcM"
API = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text2image/image-synthesis"
STYLE = ("pixel art game sprite, single object isolated on pure white background, "
         "no room no scene no people no shadow, centered, retro 16-bit pixel style, "
         "warm brown and cream retro palette")

ASSETS = [
    ("warmer", "a food warmer counter with glass display shelf"),
    ("table", "a round wooden cafe table"),
    ("chair", "a wooden cafe chair"),
    ("customer", "a cafe customer character, chibi anime style"),
    ("cat_nest", "a round cat bed cushion"),
    ("cat_pad", "a small rectangular cat mat"),
    ("floor", "wooden floor tile texture, seamless tileable pattern"),
    ("wall", "cream wall texture, seamless tileable pattern"),
]

def post(data):
    req = urllib.request.Request(API, data=json.dumps(data).encode("utf-8"),
        headers={"Content-Type": "application/json", "Authorization": "Bearer " + KEY,
                 "X-DashScope-Async": "enable"})
    with urllib.request.urlopen(req) as r:
        return json.loads(r.read().decode("utf-8"))

def get_task(task_id):
    with urllib.request.urlopen(urllib.request.Request(
        f"https://dashscope.aliyuncs.com/api/v1/tasks/{task_id}",
        headers={"Authorization": "Bearer " + KEY})) as r:
        return json.loads(r.read().decode("utf-8"))

def download(url, path):
    urllib.request.urlretrieve(url, path)

def main():
    os.makedirs("Assets/Art/raw", exist_ok=True)
    for name, desc in ASSETS:
        out = f"Assets/Art/{name}.png"
        if os.path.exists(out):
            print(f"[跳过] {name} 已存在")
            continue
        print(f"[生成] {name} ...", flush=True)
        try:
            resp = post({"model": "wanx2.1-t2i-turbo",
                         "input": {"prompt": f"{STYLE}, {desc}"},
                         "parameters": {"size": "1024*1024", "n": 1}})
            task_id = resp["output"]["task_id"]
            url = None
            for _ in range(40):
                time.sleep(4)
                tr = get_task(task_id)
                st = tr["output"]["task_status"]
                if st == "SUCCEEDED":
                    url = tr["output"]["results"][0]["url"]
                    break
                if st == "FAILED":
                    print(f"  !! {name} 任务失败: {json.dumps(tr.get('output',{}), ensure_ascii=False)[:200]}")
                    break
            if not url:
                print(f"  !! {name} 超时")
                continue
            raw = f"Assets/Art/raw/{name}.png"
            download(url, raw)
            subprocess.run(["python", "Tools/pixelize.py", raw, out,
                            "--size", "96", "--bg-tolerance", "30"], check=False)
            print(f"  完成 {name}")
        except Exception as e:
            print(f"  !! {name} 异常: {e}")
    print("全部完成")

if __name__ == "__main__":
    main()
