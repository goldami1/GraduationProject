﻿namespace InnoviApiWrapper
{
    public class SiteFolder : InnoviObject
    {
        public string Name { get; set; }
        public InnoviObjectCollection<Sensor> Sensors { get; set; }

        public void AddSensor(Sensor i_Sensor)
        {

        }
    }
}