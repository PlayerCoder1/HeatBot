using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HeatBot
{
    public class SchedulerManager
    {
        private List<TimeSlot> schedule;
        private readonly Action onStart;
        private readonly Action onStop;
        private CancellationTokenSource cancellationTokenSource;
        private bool pauseTime = false;

        private const string ScheduleFile = "planningConfig.json";

        public SchedulerManager(Action startAction, Action stopAction)
        {
            schedule = LoadSchedule();
            onStart = startAction;
            onStop = stopAction;
        }

        public void StartScheduler()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    int currentHour = DateTime.Now.Hour;
                    bool isActive = schedule.Exists(slot => slot.Hour == currentHour && slot.IsActive);

                    if (isActive && pauseTime)
                    {

                        pauseTime = false;
                        onStart?.Invoke();
                    }
                    else if (!isActive && !pauseTime)
                    {

                        pauseTime = true;
                        onStop?.Invoke();
                    }

                    Thread.Sleep(60000);
                }
            }, token);
        }

        public void StopScheduler()
        {
            cancellationTokenSource?.Cancel();
        }

        private List<TimeSlot> LoadSchedule()
        {
            if (File.Exists(ScheduleFile))
            {
                string content = File.ReadAllText(ScheduleFile);
                return JsonSerializer.Deserialize<List<TimeSlot>>(content) ?? new List<TimeSlot>();
            }

            return new List<TimeSlot>();
        }

        public void ReloadSchedule()
        {
            schedule = LoadSchedule();
        }
    }
}