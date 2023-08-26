#ifndef PIXELS_PARALLEL_REDUCTION_HEADER_INCLUDE
#define  PIXELS_PARALLEL_REDUCTION_HEADER_INCLUDE

#if defined(_THREAD_256)
    #define _THREAD 256
#elif defined(_THREAD_128)
    #define _THREAD 128
#elif defined(_THREAD_64)
    #define _THREAD 64
#elif defined(_THREAD_32)
    #define _THREAD 32
#elif defined(_THREAD_16)
    #define _THREAD 16
#elif defined(_THREAD_8)
    #define _THREAD 8
#elif defined(_THREAD_4)
    #define _THREAD 4
#elif defined(_THREAD_2)
    #define _THREAD 2
#else
    #define _THREAD 512
#endif

#define PARALLEL_REDUCTION_ADD(src, id_0, id_1)  (src[id_0] + src[id_1])
#define PARALLEL_REDUCTION_MAX(src, id_0, id_1)  (max(src[id_0], src[id_1]))
#define PARALLEL_REDUCTION_MIN(src, id_0, id_1)  (min(src[id_0], src[id_1]))

#if defined(_FUNC_ADD)
    #define PARALLEL_REDUCTION_FUNC(group_cache, index, offset) PARALLEL_REDUCTION_ADD(group_cache, index, offset)
#elif defined(_FUNC_MAX)
    #define PARALLEL_REDUCTION_FUNC(group_cache, index, offset) PARALLEL_REDUCTION_MAX(group_cache, index, offset)
#elif defined(_FUNC_MIN)
    #define PARALLEL_REDUCTION_FUNC(group_cache, index, offset) PARALLEL_REDUCTION_MIN(group_cache, index, offset)
#else
    #define PARALLEL_REDUCTION_FUNC(group_cache, index, offset) PARALLEL_REDUCTION_ADD(group_cache, index, offset)
#endif

#endif
