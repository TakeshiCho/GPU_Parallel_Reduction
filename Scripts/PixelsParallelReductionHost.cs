using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace GPU_Parallel_Reduction.Scripts
{
    public class PixelsParallelReductionHost : MonoBehaviour
    {
        public ComputeShader computeShader;
        public Texture2D texture;

        [ContextMenu("GPU Execute")]
        public int ParallelReductionWithGPU()
        {
            int[] result = new int[1]; 
            int texSize = texture.width * texture.height;
            ComputeBuffer cache = new ComputeBuffer(texture.width*texture.height/1024, sizeof(int), ComputeBufferType.Structured);
            int kernel = computeShader.FindKernel("PixelsParallelReduction");
            computeShader.SetTexture(kernel,"Source",texture);
            computeShader.SetVector("Source_Size",new Vector2(texture.width,texture.height));
            computeShader.SetBuffer(kernel,"Global_Cache",cache);
            LocalKeyword stepFirst = new LocalKeyword(computeShader,"_STEP_FIRST");
            LocalKeyword func = new LocalKeyword(computeShader,"_FUNC_ADD");
            Stopwatch stopwatch = new Stopwatch();
            computeShader.SetKeyword(func,true);
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
            Debug.Log($"Pixel Size: {texture.width} * {texture.height} | Result: {result[0]} | GPU Time: {speed.time} ms | Bandwidth: {speed.bandwidth} GB/s");
            cache.Release();
            return result[0];
        }
    
        string GetThreadKeyword(int remainingPixelCount) => remainingPixelCount switch
        {
            256 => "_THREAD_128",  128 => "_THREAD_64",  64  => "_THREAD_32",
            32  => "_THREAD_16",   16  => "_THREAD_8",   8   => "_THREAD_4", 
            4   => "_THREAD_2",    _   => "_THREAD_2"
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
            Debug.Log($"Pixel Size: {texture.width} * {texture.height} | Result: {sum} | CPU Time: {speed.time} ms | Bandwidth: {speed.bandwidth} GB/s");
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
}
