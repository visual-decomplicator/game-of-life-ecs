using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct FpsSystem : ISystem {
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        var fpsBufferLookup = SystemAPI.GetBufferLookup<FPSCounterElement>(false);
        new FPSCalcJob() {
            Dt = SystemAPI.Time.DeltaTime,
            FPSBufferLookup = fpsBufferLookup
        }.Schedule();
    }
}

[BurstCompile]
public partial struct FPSCalcJob : IJobEntity {
    public BufferLookup<FPSCounterElement> FPSBufferLookup;
    public float Dt;
    public void Execute(Entity entity, ref FPSCounterComponent fpsCounter) {
        var fpsBuffer = FPSBufferLookup[entity];
        if (fpsBuffer.Length < fpsCounter.MaxBufferElements) {
            fpsBuffer.Add(new FPSCounterElement() { Value = Dt });
            fpsCounter.CurrentBufferElement++;
            return;
        }

        if (fpsCounter.CurrentBufferElement >= fpsCounter.MaxBufferElements) {
            fpsCounter.CurrentBufferElement = 0;
        }
            
        fpsBuffer[fpsCounter.CurrentBufferElement] = new FPSCounterElement() { Value = Dt };
        fpsCounter.CurrentBufferElement++;
            
        // Calculate FPS
        float total = 0f;
        for (int i = 0; i < fpsBuffer.Length; i++) {
            total += fpsBuffer[i].Value;
        }

        fpsCounter.CurrentFPS = (int)math.round(fpsBuffer.Length / total);
    }
}