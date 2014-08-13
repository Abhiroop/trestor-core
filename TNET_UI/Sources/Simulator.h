// @Author : Arpan Jati
// @Date: 12th Aug 2014

#pragma once

#ifndef SIMULATOR_H
#define SIMULATOR_H

#include <Windows.h>
#include "Utils.h"
#include "Point2.h"
#include "Node.h"
#include "Constants.h"

#include "FakeNetwork.h"

typedef struct NodeData
{
public: Point2 Corner;
public: Point2 Center;

		NodeData()
		{

		}
} NodeData;

extern hash_map<Hash, Node> sim_nodes;
extern vector<Point2> sim_XY;
extern hash_map<Hash, NodeData> sim_nData;

class Simulator
{
	HANDLE hTimerQueue = NULL;

public:

	FakeNetwork network;

	void Initialize()
	{
		network = FakeNetwork(hTimerQueue);
	}

	Simulator()
	{
		Initialize();
	}
	
	void StartSimulation();

	



};



#endif