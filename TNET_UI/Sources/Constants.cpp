
#include "Utils.h"
#include "Constants.h"
#include "tbb/concurrent_queue.h"

hash_map<Hash, shared_ptr<Node>> GlobalNodes;

tbb::concurrent_queue<string> MessageQueue;

