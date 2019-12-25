﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Server
{
    class RSPComponent
    {
        public static void HandleRequest(Request request)
        {
            switch (request.CommandID)
            {
                case 0x32:
                    GetConfigCommand.HandleRequest(request);
                    break;

                default:
                    Log.Warn(string.Format("Unhandled request: {0} {1}", request.ComponentID, request.CommandID));
                    break;
            }
        }
    }
}
