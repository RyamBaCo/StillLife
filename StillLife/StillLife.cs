using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using EasyConfig;
using Leap;

namespace StillebenBrowser
{
    public partial class StillLife : Form
    {
        delegate void SetImageCallback(string file);
        Thread updateThread;

        LeapListener leapListener;
        Controller leapController;

        Object lockMoveValues = new Object();

        const int NumberOfRows = 25;
        const int NumberOfColumns = 70;

        const float TextureWidth = 1000.0f;
        const float TextureHeight = 833.0f;

        float speedUpDown = .1f;
        float speedLeftRight = .3f;

        bool fingersConnected = true;

        SizeF loadedTexturePosition = new SizeF(-1, -1);
        SizeF currentTexturePosition = SizeF.Empty;
        SizeF startTexturePosition = SizeF.Empty;
        SizeF idleTexturePosition = new SizeF(NumberOfRows / 2, NumberOfColumns / 2);

        public StillLife()
        {
            InitializeComponent();

            ConfigFile config = new ConfigFile(@"config.ini");
            speedUpDown = config.SettingGroups["Movement"].Settings["SpeedUpDown"].GetValueAsFloat();
            speedLeftRight = config.SettingGroups["Movement"].Settings["SpeedLeftRight"].GetValueAsFloat();

            leapController = new Controller();
            leapListener = new LeapListener();
            leapController.AddListener(leapListener);
            leapListener.LeapSwipe += new LeapListener.SwipeEvent(OnSwipe);
            leapListener.LeapRegisterFingers += new LeapListener.RegisterFingers(OnRegisterFingers);

            updateThread = new Thread(UpdateThread);
            updateThread.Start();
        }

        public void UpdateThread()
        {
            while (true)
            {
                int currentRow;
                int currentColumn;

                lock (lockMoveValues)
                {
                    currentRow = Math.Min(NumberOfRows - 1, Convert.ToInt32(currentTexturePosition.Width));
                    currentColumn = Math.Min(NumberOfColumns - 1, Convert.ToInt32(currentTexturePosition.Height));
                }

                SetImage(@"images\" + currentRow + "_" + currentColumn + ".jpg");
            }
        }

        private void SetImage(string file)
		{
			if (pictureBoxImage.InvokeRequired)
			{
                SetImageCallback d = new SetImageCallback(SetImage);
				this.Invoke(d, new object[] { file });
			}
			else
			{
                pictureBoxImage.Image = Image.FromFile(file);
                int scaledWidth = Convert.ToInt32((float)pictureBoxImage.Image.Height / Height * Width);
                pictureBoxImage.Location = new Point((Width - scaledWidth) / 2, 0);
                pictureBoxImage.Size = new Size(scaledWidth, Height);
			}
		}

        private void OnRegisterFingers(bool connected)
        {
            lock (lockMoveValues)
            {
                fingersConnected = connected;

                if (!fingersConnected)
                {
                    startTexturePosition = currentTexturePosition;
/*                    passedIdleTime = 0;
                    finalIdleTime = (startTexturePosition - idleTexturePosition).Length() / idleSpeed;*/
                }
            }
        }

        private void OnSwipe(SwipeDirection swipeDirection)
        {
            lock (lockMoveValues)
            {
                switch (swipeDirection)
                {
                    case SwipeDirection.Up:
                        currentTexturePosition.Width -= speedUpDown;
                        if (currentTexturePosition.Width < 0)
                            currentTexturePosition.Width = 0;
                        break;
                    case SwipeDirection.Down:
                        currentTexturePosition.Width += speedUpDown;
                        if (currentTexturePosition.Width >= NumberOfRows)
                            currentTexturePosition.Width = NumberOfRows - 1;
                        break;
                    case SwipeDirection.Right:
                        currentTexturePosition.Height -= speedLeftRight;
                        if (currentTexturePosition.Height < 0)
                            currentTexturePosition.Height = 0;//COLUMNS - 1;
                        break;
                    case SwipeDirection.Left:
                        currentTexturePosition.Height += speedLeftRight;
                        if (currentTexturePosition.Height >= NumberOfColumns)
                            currentTexturePosition.Height = NumberOfColumns - 1;// 0;
                        break;
                }
            }
        }

        private void Stilleben_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateThread.Abort();
        }
    }
}
