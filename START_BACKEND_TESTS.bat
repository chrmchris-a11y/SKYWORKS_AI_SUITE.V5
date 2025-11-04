@echo off
cd /d "C:\Users\chrmc\Desktop\SKYWORKS_AI_SUITE.V5\Backend_Python"
set PYTHONPATH=.
C:\Python314\python.exe -m uvicorn main:app --host 127.0.0.1 --port 8001 --reload
