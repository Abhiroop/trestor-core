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

extern hash_map<Hash, shared_ptr<Node>> sim_nodes;
extern vector<Point2> sim_XY;
extern hash_map<Hash, NodeData> sim_nData;

void CALLBACK TimerProcS(void* lpParametar, BOOLEAN TimerOrWaitFired);

class Simulator
{
	HANDLE hTimerQueue = NULL;

	HANDLE hTimer = NULL;



public:

	FakeNetwork network;

	bool Refreshed = false;

	//vector<Node> Nodes;

	void StartSimulation();
	void StopSimulation();
	void Timestep();

	void Initialize(int Resolution_MS);

	Simulator(int Resolution_MS);

	Simulator();

	bool GoodInit = false;

	bool SimulationStarted = false;

};



#endif