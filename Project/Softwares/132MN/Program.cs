using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Popolo.ThermalComfort
{
    class Program
    {
        static void Main(string[] args)
        {
            //DEBUG
            //args = new string[] { "inputs.csv" };

            if (args.Length == 0)
            {
                Console.WriteLine("Specify a filename (ex. inputs.csv)");
                return;
            }
            string filePath = args[0];
            if (! File.Exists(filePath))
            {
                Console.WriteLine("File '" + filePath + "' doesn't exist.");
                return;
            }

            //Make output file
            string oFilePath = filePath.Remove(filePath.LastIndexOf('.')) + "_Result.csv";

            //Read input file
            using (StreamReader sReader = new StreamReader(filePath))
            using(StreamWriter sWriter = new StreamWriter(oFilePath, false, Encoding.GetEncoding("Shift_JIS")))
            {
                //Read "weight"
                double weight = double.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "height"
                double height = double.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "age"
                double age = double.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "sex"
                bool isMale = bool.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "cardiac index at rest"
                double ci = double.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "fat percentage"
                double fat = double.Parse(sReader.ReadLine().Split(',')[1]);
                //Read "output time span"
                double oSpan = double.Parse(sReader.ReadLine().Split(',')[1]);

                //Make an instance of HumanBody class
                HumanBody body = new HumanBody(weight, height, age, isMale, ci, fat);
                //Output informations
                outputData(sWriter, body, 0, true);
                outputData(sWriter, body, 0, false);

                sReader.ReadLine();
                sReader.ReadLine();

                //Start iteration
                double cTime = 0;
                double time =0;
                double nextTime = 0;
                Dictionary<HumanBody.Nodes, double> dbTemp = new Dictionary<HumanBody.Nodes, double>();  //乾球温度
                Dictionary<HumanBody.Nodes, double> mrTemp = new Dictionary<HumanBody.Nodes, double>(); //平均放射温度
                Dictionary<HumanBody.Nodes, double> vels = new Dictionary<HumanBody.Nodes, double>();   //気流速度
                Dictionary<HumanBody.Nodes, double> rHumid = new Dictionary<HumanBody.Nodes, double>(); //相対湿度
                Dictionary<HumanBody.Nodes, double> matTemp = new Dictionary<HumanBody.Nodes, double>();//物体温度
                Dictionary<HumanBody.Nodes, double> conRate = new Dictionary<HumanBody.Nodes, double>();//接触割合
                Dictionary<HumanBody.Nodes, double> conduct = new Dictionary<HumanBody.Nodes, double>();//接触部の熱コンダクタンス
                Dictionary<HumanBody.Nodes, double> dbTempNext = new Dictionary<HumanBody.Nodes, double>();  //乾球温度
                Dictionary<HumanBody.Nodes, double> mrTempNext = new Dictionary<HumanBody.Nodes, double>(); //平均放射温度
                Dictionary<HumanBody.Nodes, double> velsNext = new Dictionary<HumanBody.Nodes, double>();   //気流速度
                Dictionary<HumanBody.Nodes, double> rHumidNext = new Dictionary<HumanBody.Nodes, double>(); //相対湿度
                Dictionary<HumanBody.Nodes, double> matTempNext = new Dictionary<HumanBody.Nodes, double>();//物体温度
                Dictionary<HumanBody.Nodes, double> conRateNext = new Dictionary<HumanBody.Nodes, double>();//接触割合
                Dictionary<HumanBody.Nodes, double> conductNext = new Dictionary<HumanBody.Nodes, double>();//接触部の熱コンダクタンス

                //Read boundary data at time 0
                readData(sReader.ReadLine(), ref time, ref dbTemp, ref mrTemp, ref vels, ref rHumid, ref matTemp, ref conRate, ref conduct);
                readData(sReader.ReadLine(), ref nextTime, ref dbTempNext, ref mrTempNext, ref velsNext, ref rHumidNext, ref matTempNext, ref conRateNext, ref conductNext);

                while (true)
                {
                    if (nextTime < cTime)
                    {
                        string line = sReader.ReadLine();
                        if (line == null) break;
                        if (line.Split(',')[0] == "") break;
                        time = nextTime;
                        Dictionary<HumanBody.Nodes, double> buff;
                        buff = dbTemp; dbTemp = dbTempNext; dbTempNext = buff;
                        buff = mrTemp; mrTemp = mrTempNext; mrTempNext = buff;
                        buff = rHumid; rHumid = rHumidNext; rHumidNext = buff;
                        buff = vels; vels = velsNext; velsNext = buff;
                        buff = matTemp; matTemp = matTempNext; matTempNext = buff;
                        buff = conRate; conRate = conRateNext; conRateNext = buff;
                        buff = conduct; conduct = conductNext; conductNext = buff;
                        readData(line, ref nextTime, ref dbTempNext, ref mrTempNext, ref velsNext, ref rHumidNext, ref matTempNext, ref conRateNext, ref conductNext);
                    }
                    else
                    {
                        //Set boundary conditions
                        foreach (HumanBody.Nodes key in dbTemp.Keys)
                        {
                            body.SetDrybulbTemperature(key, interpolate(time, nextTime, dbTemp[key], dbTempNext[key], cTime));
                            body.SetMeanRadiantTemperature(key, interpolate(time, nextTime, mrTemp[key], mrTempNext[key], cTime));
                            body.SetVelocity(key, interpolate(time, nextTime, vels[key], velsNext[key], cTime));
                            body.SetRelativeHumidity(key, interpolate(time, nextTime, rHumid[key], rHumidNext[key], cTime));
                            body.SetMaterialTemperature(key, interpolate(time, nextTime, matTemp[key], matTempNext[key], cTime));                            
                            body.SetContactPortionRate(key, interpolate(time, nextTime, conRate[key], conRateNext[key], cTime));
                            body.SetHeatConductanceToMaterial(key, interpolate(time, nextTime, conduct[key], conductNext[key], cTime));
                        }
                        
                        //Update Human body
                        body.Update(oSpan);

                        //Update current time
                        cTime += oSpan;

                        //Output informations to file
                        outputData(sWriter, body, cTime, false);

                        //Output informations to console
                        Console.WriteLine(cTime + " sec");
                    }
                }
            }
        }

        private static void readData(
            string line,
            ref double time,
            ref Dictionary<HumanBody.Nodes, double> dbTemp,
            ref Dictionary<HumanBody.Nodes, double> mrTemp,
            ref Dictionary<HumanBody.Nodes, double> vels,
            ref Dictionary<HumanBody.Nodes, double> rHumid,
            ref Dictionary<HumanBody.Nodes, double> matTemp,
            ref Dictionary<HumanBody.Nodes, double> conRate,
            ref Dictionary<HumanBody.Nodes, double> conduct)
        {
            string[] st = line.Split(',');
            time = double.Parse(st[0]);

            Dictionary<HumanBody.Nodes, double>[] db = new Dictionary<HumanBody.Nodes, double>[] { dbTemp, mrTemp, vels, rHumid, matTemp, conRate, conduct};
            for (int i = 0; i < db.Length; i++)
            {
                int index = i * 17 + 1;
                db[i][HumanBody.Nodes.Head] = double.Parse(st[index]);
                db[i][HumanBody.Nodes.Neck] = double.Parse(st[index + 1]);
                db[i][HumanBody.Nodes.Chest] = double.Parse(st[index + 2]);
                db[i][HumanBody.Nodes.Back] = double.Parse(st[index + 3]);
                db[i][HumanBody.Nodes.Pelvis] = double.Parse(st[index + 4]);
                db[i][HumanBody.Nodes.LeftShoulder] = double.Parse(st[index + 4]);
                db[i][HumanBody.Nodes.LeftArm] = double.Parse(st[index + 5]);
                db[i][HumanBody.Nodes.LeftHand] = double.Parse(st[index + 6]);
                db[i][HumanBody.Nodes.RightShoulder] = double.Parse(st[index + 7]);
                db[i][HumanBody.Nodes.RightArm] = double.Parse(st[index + 8]);
                db[i][HumanBody.Nodes.RightHand] = double.Parse(st[index + 9]);
                db[i][HumanBody.Nodes.LeftThigh] = double.Parse(st[index + 10]);
                db[i][HumanBody.Nodes.LeftLeg] = double.Parse(st[index + 11]);
                db[i][HumanBody.Nodes.LeftFoot] = double.Parse(st[index + 12]);
                db[i][HumanBody.Nodes.RightThigh] = double.Parse(st[index + 13]);
                db[i][HumanBody.Nodes.RightLeg] = double.Parse(st[index + 14]);
                db[i][HumanBody.Nodes.RightFoot] = double.Parse(st[index + 15]);
            }
        }

        private static double interpolate(double x1, double x2, double y1, double y2, double x)
        {
            return y1 + (x - x1) * (y2 - y1) / (x2 - x1);
        }

        private static void outputData(StreamWriter sWriter, HumanBody body, double time, bool isTitleLine)
        {
            ImmutableBodyPart[] parts = body.GetBodyPart();

            if (isTitleLine)
            {
                sWriter.Write("Time[sec],");
                foreach (ImmutableBodyPart part in parts)
                {
                    sWriter.Write(part.Position + "-Core,");
                    sWriter.Write(part.Position + "-Muscle,");
                    sWriter.Write(part.Position + "-Fat,");
                    sWriter.Write(part.Position + "-Skin,");
                    sWriter.Write(part.Position + "-Artery,");
                    sWriter.Write(part.Position + "-DeepVein,");
                }
            }
            else
            {
                sWriter.Write(time + ",");
                foreach (ImmutableBodyPart part in parts)
                {
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.Core) + ",");
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.Muscle) + ",");
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.Fat) + ",");
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.Skin) + ",");
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.Artery) + ",");
                    sWriter.Write(part.GetTemperature(BodyPart.Segments.DeepVein) + ",");
                }
            }
            sWriter.WriteLine();
        }

    }

}
