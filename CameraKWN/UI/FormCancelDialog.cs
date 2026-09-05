using System.Drawing;
using System.Windows.Forms;

namespace camera_show {
    /// <summary>Small "cancel / set-port" dialog shown while the app waits for fMain to hand off a job.</summary>
    public class FormCancelDialog {
        public Form Form { get; private set; } = new Form();

        public void Show(Form1 main) {
            Form = new Form {
                Icon       = Properties.Resources.icon,
                Size       = new Size(200, 70),
                ControlBox = false,
                Text       = main.global.StepTest
            };

            var cancelBtn = new Button {
                Text     = "cancel",
                Size     = new Size(75, 30),
                Location = new Point(0, 0)
            };
            cancelBtn.Click += main.ButtonCancelClick;

            var setPortBtn = new Button {
                Text     = AppText.SetPort,
                Size     = new Size(75, 30),
                Location = new Point(80, 0)
            };
            setPortBtn.Click += main.ButtonSetPortClick;

            Form.Controls.Add(cancelBtn);
            Form.Controls.Add(setPortBtn);
            Form.Show();
        }
    }
}
