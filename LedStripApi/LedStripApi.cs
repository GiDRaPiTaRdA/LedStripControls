using System;
using System.Collections.Generic;
using System.Text;

namespace LedStripApi
{
    public class LedStrip
    {
        public IConnection Connection { get; }

        public LedStrip(IConnection connection)
        {
            this.Connection = connection;
        }

        public void SetBrightness(byte value) => this.SendCommand($"brightness {value}");

        public void SetSpeed(byte value) => this.SendCommand($"speed {value}");

        public void SetColor(int color) => this.SendCommand($"color {color}");

        public void SetAnimation(int animationId) => this.SendCommand($"animation {animationId}");

        public void SetBlending(EBlending blending) => this.SendCommand($"blend {blending}");

        public void Close() => this.SendCommand("close");

        public void SendCommand(string command)
        {
            this.Connection.SendCommand(command);
        }
    }
}
