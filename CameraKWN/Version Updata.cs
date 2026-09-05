using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace camera_show {
    class Version_Updata {
        //V1.02
        //Edit code to read easy
        //Edit file config from txt to csv
        //Edit function scan min max value camera to scan fast
        //Edit SmallChange set camera
        //Edit bug scan min max value camera
        //Add check digit sn
        //Add adjust degree
        //Add check led add step

        //V1.03
        //Add step fail in result.txt example "(3)OverTime\r\nFAIL"

        //V2022.04
        //Add path head 1 2 3 4

        //V2022.05
        //Add blinkLed mode

        //V2022.06
        //Add ReAdjust Camera

        //V2022.07
        //Edit set gamma to address camera

        //V2022.08
        //Edit set gamma and gain to address camera

        //V2022.09
        //Add saturation to address

        //V2022.10
        //Remove adjust camera in sat camera
        //Add save config to get config camera to csv

        //V2022.11
        //Add use port to open camera with port

        //V2023.01
        //Edit path log error CameraSetCapProp

        //V2023.02
        //Add log program catch all process

        //V2023.03
        //ลองเทสแล้วต้องใช้คู่กันถึงจะใช้ได้ทั้งตอนรันโค้ดและรัน exe อันล่างให้เอาไปใส่ใน Program.cs
        //AppDomain.CurrentDomain.UnhandledException 
        //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

        //V2023.04
        //Remove address gain

        //V2023.05
        //Add process timeDiv3 in read2d, checkLED, comparImage

        //V2023.06
        //ถ้ามันหา port ไม่เจอ ให้ตีเฟวได้เลย สำหรับไม่ใช่ดีบักโหมด

        //V2023.07
        //Edit image icon

        //V2023.08
        //เปลี่ยนวิธีการปรับกล้องใหม่ ตอนแรกเอา timeout มาหาร 3 แล้วแบ่งเป็น 2 ช่วง
        //ก็คือเปลี่ยนตอนช่วง 2 ให้นับต่อจากช่วงแรงไป 1 วินาที แล้วเข้าช่วง 2 ได้เลย

        //V2023.09
        //แก้เขียนคำผิด compare

        //V2023.10
        //add txt file config roi crop

        //2023.11
        //ตอนที่ set port ถ้าใน combobox เป็นค่าว่าง มันจะใส่ 0 เข้าไปแทน

        //2023.12
        //แก้บัค ตอนหา address ไม่เจอ มันจะวนเรื้อยๆ

        //2026.01
        //Major code reorganization using AI.
    }
}
