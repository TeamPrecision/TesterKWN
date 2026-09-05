import os
import sys
import subprocess

if len(sys.argv) < 2:
    print("COM port parameter is missing.")
    #sys.exit(1)
    port = "0x0957::0x4D18"

else:
    port = sys.argv[1]

# หาพาทของไฟล์ Python script ปัจจุบัน
current_directory = os.path.dirname(os.path.abspath(__file__))

# กำหนดชื่อของโปรแกรม exe และพาทที่คุณต้องการใช้
exe_name = "VisaNi.exe"
exe_path = os.path.join(current_directory, "..", exe_name)

# กำหนดพารามิเตอร์ที่ต้องการส่งไปยังโปรแกรม exe
parameters = ["param1", "param2", "param3"]

# ใช้ subprocess เรียกโปรแกรม exe พร้อมส่งพารามิเตอร์
process = subprocess.Popen([exe_path], stdout=subprocess.PIPE, stderr=subprocess.PIPE)

# รอให้โปรแกรม exe ทำงานเสร็จสิ้น
stdout, stderr = process.communicate()

# แสดงผลลัพธ์ที่ได้จากโปรแกรม exe
print(stdout.decode())

# ใช้ split เพื่อแยกข้อความโดยใช้ \n เป็นตัวแยก
lines = stdout.decode().split("\r\n")

# ตรวจสอบว่ามี port ที่ต้องการไหม
portFull = ""
for line in lines:
    if port in line:
        portFull = line

if portFull == "":
    print("Not found")
else:
    print(portFull)
