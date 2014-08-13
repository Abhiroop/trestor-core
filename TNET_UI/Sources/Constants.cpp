

#include "Constants.h"
#include "tbb/concurrent_queue.h"


hash_map<Hash, Node*> GlobalNodes;

tbb::concurrent_queue<string> MessageQueue;

