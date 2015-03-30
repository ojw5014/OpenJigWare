using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace OpenJigWare
{
    partial class Ojw
    {
        public class CSock
        {
            public CSock()
            {
            }
            private bool m_bClassEnd = false;
            ~CSock()
            {
                if (m_bClassEnd == true) m_bClassEnd = true;
                //if (IsConnect())
                //    DisConnect();
            }
        }
    }
}
