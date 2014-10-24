

#include "NTPClient.h"


#include <winsock2.h>
#include <winsock.h>
#include <ws2tcpip.h>


void Timestamp::ReverseEndian(void) {
	ReverseEndianInt(seconds);
	ReverseEndianInt(fraction);
}

time_t Timestamp::to_time_t(void) {
	return (seconds - ((70 * 365 + 17) * 86400)) & 0x7fffffff;
}

void NTPMessage::ReverseEndian(void) {
	ref.ReverseEndian();
	orig.ReverseEndian();
	rx.ReverseEndian();
	tx.ReverseEndian();
}

int NTPMessage::recv(int sock) {
	int ret = ::recv(sock, (char*)this, sizeof(*this), 0);
	ReverseEndian();
	return ret;
}

int NTPMessage::sendto(int sock, struct sockaddr_in* srv_addr) {
	ReverseEndian();
	int ret = ::sendto(sock, (const char*)this, sizeof(*this), 0, (sockaddr*)srv_addr, sizeof(*srv_addr));
	ReverseEndian();
	return ret;
}

void NTPMessage::clear() {
	memset(this, 0, sizeof(*this));
}