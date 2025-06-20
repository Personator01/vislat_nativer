// See https://aka.ms/new-console-template for more information
using BCI2000RemoteNET;
using Raylib_cs;

const double TRIAL_TIME = 0.25;
const int WIN_SIZE = 400;

BCI2000Connection conn = new();
conn.Synchronized = true;

conn.Connect("127.0.0.1", 3999);

BCI2000Remote bci = new(conn);



bci.AddEvent("Ev", 1, 0);
bci.AddEvent("IntertrialSync", 1, 0);

bci.StartupModules( new Dictionary<string, IEnumerable<string>?>{
        {"Blackrock", null },
        {"DummySignalProcessing", null},
        {"DummyApplication", null}
        });

bci.SetParameter("SamplingRate", "30000");

bci.Visualize("Ev");
bci.Visualize("TimestampDifference");

Raylib.InitWindow(WIN_SIZE, WIN_SIZE, "BCI2000RemoteNET latency test");


while (bci.GetSystemState() != BCI2000Remote.SystemState.Running) {
    Thread.Sleep(100);
}

int trials_count = 0;
const int TRIALS_PER = 20;

double timer2 = 0;
bool hasSentSyncEv = false;

double timeElapsed = 0;
Color rectColor = Color.Black;
while (!Raylib.WindowShouldClose()) {
  if (trials_count >= TRIALS_PER)
  {
    timer2 += Raylib.GetFrameTime();
    if (timer2 > TRIAL_TIME * 4)
    {
      if (!hasSentSyncEv)
      {
        bci.PulseEventUnchecked("IntertrialSync", 1);
        hasSentSyncEv = true;
        timer2 = 0;
      }
      else
      {
        trials_count = 0;
        timer2 = 0;
        hasSentSyncEv = false;
      }
    } 
  }
  else
  {


    timeElapsed += Raylib.GetFrameTime();
    if (timeElapsed >= TRIAL_TIME)
    {
      timeElapsed = 0;
      if (rectColor.Equals(Color.Black))
      {
        rectColor = Color.White;
        bci.SetEventTimestamped("Ev", 1, execute_unchecked: true);
      }
      else
      {
        rectColor = Color.Black;
        bci.SetEventTimestamped("Ev", 0, execute_unchecked: true);
      }
    }
  }

    Raylib.BeginDrawing();

    Raylib.ClearBackground(Color.Black);

    Raylib.DrawRectangle(0,20,200,180,rectColor);

    Raylib.DrawFPS(0,0);

    Raylib.EndDrawing();
}
