using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core
{
    [DataContract]
    public struct Size
    {
        [DataMember]
        public int Width;

        [DataMember]
        public int Height;

        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public static Size Zero
        {
            get
            {
                return new Size()
                {
                    Width = 0,
                    Height = 0,
                };
            }
        }

    }
}
