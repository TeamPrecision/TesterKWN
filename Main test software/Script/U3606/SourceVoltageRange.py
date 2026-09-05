import os
import sys
import time
import subprocess

if len(sys.argv) < 2:
    print("COM port parameter is missing.")
    #sys.exit(1)
    port = "USB0::0x0957::0x4D18::MY61450016::INSTR"
    range = "S1"

else:
    port = sys.argv[1]
    range = sys.argv[2]

if range == "S1":
    range = "30"

elif range == "S2":
    range = "8"

elif range == "S2m":
    range = "1"

elif range == "S1S2":
    range = "AUTO"

else:
    print(f"Paramitor Range {range} Error")
    sys.exit()

# S1 | S2 | S2m | S1S2
command = f"SOUR:VOLT:RANG {range}"

# หาพาทของไฟล์ Python script ปัจจุบัน
current_directory = os.path.dirname(os.path.abspath(__file__))

# กำหนดชื่อของโปรแกรม exe และพาทที่คุณต้องการใช้
exe_name = "VisaNi.exe"
exe_path = os.path.join(current_directory, "..", exe_name)

# กำหนดพารามิเตอร์ที่ต้องการส่งไปยังโปรแกรม exe
parameters = [port, command]

# ใช้ subprocess เรียกโปรแกรม exe พร้อมส่งพารามิเตอร์
process = subprocess.Popen([exe_path] + parameters, stdout=subprocess.PIPE, stderr=subprocess.PIPE)

# รอให้โปรแกรม exe ทำงานเสร็จสิ้น
stdout, stderr = process.communicate()

# แสดงผลลัพธ์ที่ได้จากโปรแกรม exe
print(stdout.decode())