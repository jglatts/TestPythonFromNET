"""
infer.py

Python script for real-time image processing with C# integration.

- Reads image file paths from stdin (or a single test image in DEBUG mode).
- Draws overlays (currently a red circle at fixed coordinates) on each frame.
- Converts processed frames to base64 JPEG and outputs JSON to stdout for C#.
- Can be extended to include actual detection, anomaly scoring, or other image processing.

"""
import sys
import json
import cv2
import base64
import numpy as np


DEBUG = False
ran_from_stdin = False

def encode_image(img):
    _, buffer = cv2.imencode(".jpg", img)
    return base64.b64encode(buffer).decode("utf-8")


def getImg(path):
    frame = cv2.imread(path)
    if frame is None:
        print(json.dumps({"error": "cannot read frame"}), flush=True)


def getOutput(overlay):
    out = {
        "status": "TEST",
        "score": 1,
        "overlay": encode_image(overlay)
    }
    return out


def addToImg(img):
    h, w = img.shape[:2]
    cv2.circle(img, (100, 100), 20, (0, 0, 255), 3)


def analayzeSingleImg(path):
    frame = cv2.imread(path)
    if frame is None:
        print(json.dumps({"error": "cannot read frame"}), flush=True)
        return

    overlay = frame.copy()
    addToImg(overlay)
    out = getOutput(overlay)

    print(json.dumps(out), flush=True)


def getImgsFromStdIn():
    print("running", flush=True)
    for line in sys.stdin:
        path = line.strip()
        if not path:
            continue

        if path.lower() == "exit":
            break

        frame = cv2.imread(path)
        if frame is None:
            print(json.dumps({"error": "cannot read frame"}), flush=True)
            continue

        overlay = frame.copy()
        addToImg(overlay)
        out = getOutput(overlay)

        print(json.dumps(out), flush=True)

def test():
    if DEBUG:
        img_path = 'C:\\Users\\jglatts\\Documents\\Z-Axis\\YOLO-CV\\test_images\\good_1.png'
        analayzeSingleImg(img_path)
    else:
        getImgsFromStdIn()


test()
