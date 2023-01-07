
using AForge.Video;
using AForge.Video.DirectShow;
using Ia_reco_advanced;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace IA_reco
{
    public class camera
    {
        private FilterInfoCollection WebcamColl;
        public VideoCaptureDevice Device;
        IA ia = new();
        Form1 myform;

        public camera(Form1 fom)
        {
            myform=fom;
        }

        public void prendrephotoAsync()
        {
            WebcamColl = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            Device = new VideoCaptureDevice(WebcamColl[0].MonikerString);
         //   Device
            Device.Start();
            Device.NewFrame += Device_NewFrame;
        }

        private async void Device_NewFrame(object sender, NewFrameEventArgs e)
        {    
            Bitmap Bmp = (Bitmap)e.Frame.Clone();
            myform.changeimg(ia.record(Bmp));
            Bmp.Dispose();

        }

        public void eteindre()
        {
            Device.NewFrame -= Device_NewFrame;
            Device.SignalToStop();
            Device = null;
        }
    }
}
