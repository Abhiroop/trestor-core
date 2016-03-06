#include <stdio.h>
#include "PrivateKeyManage.h"
#include "Base64.h"
#include "Structs.h"


class OnionConnectionEstablish
{
	int connect(onion_peer_entry *enrties, int peer_count);
};