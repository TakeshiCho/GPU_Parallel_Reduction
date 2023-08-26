using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class PixelsSum : MonoBehaviour
{
    public ComputeShader computeShader;
    public Texture2D texture;

    [ContextMenu("GPU Execute")]
    public int ParallelReductionWithGPU()
    {
        int[] result = new int[1]; 
        int texSize = texture.width * texture.height;
        ComputeBuffer cache = new ComputeBuffer(texture.width*texture.height/1024, sizeof(int), ComputeBufferType.Structured);
        int kernel = computeShader.FindKernel("PixelsSumProcess");
        computeShader.SetTexture(kernel,"Source",texture);
        computeShader.SetVector("Source_Size",new Vector4(texture.width,texture.height,texSize,0));
        computeShader.SetBuffer(kernel,"GlobalCache",cache);
        LocalKeyword stepFirst = new LocalKeyword(computeShader,"_STEP_FIRST");

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        for (int i = texSize; i >= 1024;)
        {
            if (i == texSize) 
                computeShader.SetKeyword(stepFirst,true);
            else 
                computeShader.SetKeyword(stepFirst,false);
            computeShader.SetInt("current_count",i);
            i /= 1024;
            computeShader.Dispatch(kernel, i, 1, 1);

            if (i < 1024 && i != 1)
            {
                LocalKeyword thread = new LocalKeyword(computeShader,GetThreadKeyword(i));
                computeShader.SetKeyword(thread,true);
                computeShader.SetInt("current_count",i);
                computeShader.Dispatch(kernel, 1, 1, 1);
                computeShader.SetKeyword(thread,false);
            }
        }
        
        cache.GetData(result);

        stopwatch.Stop();
        SpeedMeasurement speed = new SpeedMeasurement(stopwatch.ElapsedTicks, (long)texSize * 4);
        Debug.Log($"Pixel Sum: {result[0]}; GPU spend: {speed.time} ms; Bandwidth: {speed.bandwidth} GB/s");
        
        cache.Release();
        return result[0];
    }
    
    string GetThreadKeyword(int remainingPixelCount) => remainingPixelCount switch
    {
        256 => "_Thread_128",  128 => "_Thread_64",  64  => "_Thread_32",
        32  => "_Thread_16",   16  => "_Thread_8",   8   => "_Thread_4", 
        4   => "_Thread_2",    2   => "_Thread_1",   _   => "_Thread_1"
    };

    [ContextMenu("CPU Execute")]
    public int LoopWithCPU()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        int sum = 0;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                sum += (int)(texture.GetPixel(x, y).r * 255);
            }
        }
        
        stopwatch.Stop();
        int texSize = texture.width * texture.height;
        SpeedMeasurement speed = new SpeedMeasurement(stopwatch.ElapsedTicks, texSize * 4);
        Debug.Log($"Pixel Sum: {sum}; CPU spend: {speed.time} ms; Bandwidth: {speed.bandwidth} GB/s");
        return sum;
    }

    struct SpeedMeasurement
    {
        public readonly float time;
        public readonly string bandwidth;

        public SpeedMeasurement(long elapsedTicks, long dataSize)
        {
            time = (float)elapsedTicks / 10_000;
            bandwidth = ((double)dataSize / 1_073_741_824 / ((double)elapsedTicks / 10_000_000)).ToString("F3");
        }
    }
}
