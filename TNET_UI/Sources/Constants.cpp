
#include "Constants.h"

concurrent_hash_map<Hash, shared_ptr<Node>> GlobalNodes;

tbb::concurrent_queue<string> MessageQueue;

concurrent_hash_map<Hash, AccountInfo> GLOBAL_LEDGER_MAP;

string Constants::hexChars = "0123456789ABCDEF";

