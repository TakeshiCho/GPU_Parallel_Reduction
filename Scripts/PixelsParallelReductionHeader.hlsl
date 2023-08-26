#ifndef PIXELS_PARALLEL_REDUCTION_HEADER_INCLUDE
#define  PIXELS_PARALLEL_REDUCTION_HEADER_INCLUDE

#if defined(_THREAD_256)
    #define __thread__ 256
#elif defined(_THREAD_128)
    #define __thread__ 128
#elif defined(_THREAD_64)
    #define __thread__ 64
#elif defined(_THREAD_32)
    #define __thread__ 32
#elif defined(_THREAD_16)
    #define __thread__ 16
#elif defined(_THREAD_8)
    #define __thread__ 8
#elif defined(_THREAD_4)
    #define __thread__ 4
#elif defined(_THREAD_2)
    #define __thread__ 2
#else
    #define __thread__ 512
#endif

#define PARALLEL_REDUCTION_ADD(src, id0, id1)  (src[id0] + src[id1])
#define PARALLEL_REDUCTION_MAX(src, id0, id1)  (max(src[id0],src[id1]))
#define PARALLEL_REDUCTION_MIN(src, id0, id1)  (min(src[id0],src[id1]))

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
