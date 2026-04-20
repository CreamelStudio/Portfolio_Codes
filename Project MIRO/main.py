from fastapi import FastAPI, HTTPException, Body
from typing import Any
import json
import os

app = FastAPI()

DATA_DIR = "data"
os.makedirs(DATA_DIR, exist_ok=True)

def save_to_file(filename: str, data: Any):
    filepath = os.path.join(DATA_DIR, f"{filename}.json")
    with open(filepath, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

def load_from_file(filename: str) -> Any:
    filepath = os.path.join(DATA_DIR, f"{filename}.json")
    if not os.path.exists(filepath):
        raise HTTPException(status_code=404, detail="File not found")
    with open(filepath, "r", encoding="utf-8") as f:
        return json.load(f)

@app.post("/save/{filename}")
async def save_item(filename: str, item: Any = Body(...)):
    save_to_file(filename, item)
    return {"message": "Data saved successfully", "filename": filename}

@app.get("/load/{filename}")
async def load_item(filename: str):
    data = load_from_file(filename)
    return data