#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AI 图后处理管线：像素化 + 抠背景变透明。

用法:
  python pixelize.py <输入.png> <输出.png> [--size 64] [--bg-tolerance 30]

流程:
  1. 缩放到 size×size（最近邻）—— 制造像素颗粒感
  2. 放大回原尺寸（最近邻）—— 像素块清晰
  3. 从四角 flood fill 抠掉近似背景色 —— 变透明
  4. 保存带 alpha 的 PNG
"""
import sys
import argparse
from collections import deque
from PIL import Image

def pixelize_and_cut(input_path, output_path, size=64, bg_tolerance=30):
    img = Image.open(input_path).convert("RGBA")

    # 1) 像素化：先缩小再放大（NEAREST 保持硬边）
    small = img.resize((size, size), Image.NEAREST)
    big = small.resize((img.width, img.height), Image.NEAREST)

    px = big.load()
    w, h = big.size

    # 2) 背景色：取四角平均
    corners = [(3, 3), (w - 4, 3), (3, h - 4), (w - 4, h - 4)]
    bg = tuple(
        sum(px[x, y][i] for x, y in corners) // len(corners)
        for i in range(3)
    )

    # 3) 从边缘 flood fill 抠背景
    def is_bg(p):
        r, g, b, a = p
        return a == 0 or (abs(r - bg[0]) <= bg_tolerance and
                          abs(g - bg[1]) <= bg_tolerance and
                          abs(b - bg[2]) <= bg_tolerance)

    visited = [[False] * w for _ in range(h)]
    queue = deque()

    # 从四边所有像素开始 flood
    for x in range(w):
        for y in (0, h - 1):
            if is_bg(px[x, y]):
                queue.append((x, y)); visited[y][x] = True
    for y in range(h):
        for x in (0, w - 1):
            if is_bg(px[x, y]) and not visited[y][x]:
                queue.append((x, y)); visited[y][x] = True

    while queue:
        x, y = queue.popleft()
        px[x, y] = (0, 0, 0, 0)  # 透明
        for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h and not visited[ny][nx]:
                if is_bg(px[nx, ny]):
                    visited[ny][nx] = True
                    queue.append((nx, ny))

    big.save(output_path)
    print(f"[OK] {input_path} -> {output_path} (pixelized {size}px + bg cut)")
    return output_path

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("input")
    parser.add_argument("output")
    parser.add_argument("--size", type=int, default=64)
    parser.add_argument("--bg-tolerance", type=int, default=30)
    args = parser.parse_args()
    pixelize_and_cut(args.input, args.output, args.size, args.bg_tolerance)
