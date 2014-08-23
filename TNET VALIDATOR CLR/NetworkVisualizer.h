
/*
*
*  @Author: Arpan Jati
*  @Version: 1.0
*  @Date: 7th August 2014
*  @Description: Network Visualizer
*
*/

#pragma once

#include "Utils.h"
#include "Point2.h"
#include "Node.h"
#include "Constants.h"
#include <hash_map>

#include "Simulator.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::Collections::Generic;

namespace TNETVALIDATORCLR {

	/// <summary>
	/// Summary for NetworkVisualizer
	/// </summary>
	public ref class NetworkVisualizer : public System::Windows::Forms::UserControl
	{
	public:

		NetworkVisualizer(void)
		{
			this->SetStyle(ControlStyles::DoubleBuffer |
				ControlStyles::UserPaint |
				ControlStyles::AllPaintingInWmPaint,
				true);

			this->UpdateStyles();

			//sim_XY.push_back(Point2((int)(this->Width * (1.0 / 2.0)), (int)(this->Height * (1.0 / 2.0))));
			//sim_XY.push_back(Point2((int)(this->Width * (1.0 / 3.0)), (int)(this->Height * (1.0 / 3.0))));
			//sim_XY.push_back(Point2((int)(this->Width * (2.0 / 3.0)), (int)(this->Height * (2.0 / 3.0))));
			
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~NetworkVisualizer()
		{
			if (components)
			{
				delete components;
			}
		}

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->SuspendLayout();
			// 
			// NetworkVisualizer
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Name = L"NetworkVisualizer";
			this->Size = System::Drawing::Size(373, 174);
			this->Load += gcnew System::EventHandler(this, &NetworkVisualizer::NetworkVisualizer_Load);
			this->Resize += gcnew System::EventHandler(this, &NetworkVisualizer::NetworkVisualizer_Resize);
			this->ResumeLayout(false);

		}
#pragma endregion

	private: System::Void NetworkVisualizer_Load(System::Object^  sender, System::EventArgs^  e) {


	}

	private: System::Void NetworkVisualizer_Resize(System::Object^  sender, System::EventArgs^  e) {

		sim_XY.clear();
		sim_nData.clear();
		
		sim_XY.push_back(Point2((int)(this->Width * (2.0 / 5.0)), (int)(this->Height * (1.0 / 5.0))));
		sim_XY.push_back(Point2((int)(this->Width * (3.0 / 5.0)), (int)(this->Height * (1.0 / 5.0))));

		sim_XY.push_back(Point2((int)(this->Width * (1.0 / 5.0)), (int)(this->Height * (2.0 / 5.0))));
		sim_XY.push_back(Point2((int)(this->Width * (4.0 / 5.0)), (int)(this->Height * (2.0 / 5.0))));

		sim_XY.push_back(Point2((int)(this->Width * (1.0 / 5.0)), (int)(this->Height * (3.0 / 5.0))));
		sim_XY.push_back(Point2((int)(this->Width * (4.0 / 5.0)), (int)(this->Height * (3.0 / 5.0))));

		sim_XY.push_back(Point2((int)(this->Width * (2.0 / 5.0)), (int)(this->Height * (4.0 / 5.0))));
		sim_XY.push_back(Point2((int)(this->Width * (3.0 / 5.0)), (int)(this->Height * (4.0 / 5.0))));

	}

	public: void OnPaint(PaintEventArgs^ e) override
	{
		Graphics^ g = e->Graphics;

		g->SmoothingMode = System::Drawing::Drawing2D::SmoothingMode::AntiAlias;
		//g->TextRenderingHint = System::Drawing::Text::TextRenderingHint::AntiAlias;

		g->Clear(Color::GhostWhite);

		int i = 0;
		for (concurrent_hash_map<Hash, shared_ptr<Node>>::iterator _ts = GlobalNodes.begin(); _ts != GlobalNodes.end(); ++_ts)
		{
			shared_ptr<Node> ND;
			ND = _ts->second;
			if (sim_nData.count(ND->PublicKey) == 0)
			{
				NodeData d;
				Point2 s = Point2(sim_XY[i].X - 10, sim_XY[i].Y - 10);
				d.Center = s;
				d.Corner = sim_XY[i];
				sim_nData[ND->PublicKey] = d;
				i++;
			}
		}


		// Draw Connections
		for (concurrent_hash_map<Hash, shared_ptr<Node>>::iterator _ts = GlobalNodes.begin(); _ts != GlobalNodes.end(); ++_ts)
		{
			shared_ptr<Node> ND;
			ND = _ts->second;
			NodeData* nd = &sim_nData[_ts->first];

			for (hash_map<Hash, shared_ptr<Node>>::iterator links = ND->Connections.begin(); links != ND->Connections.end(); ++links)
			{
				shared_ptr<Node> Link;
				Link = links->second;
				NodeData* LinkData = &sim_nData[Link->PublicKey];

				g->DrawLine(gcnew Pen(Color::LightPink, 0.5F), Point(LinkData->Center.X, LinkData->Center.Y), Point(nd->Center.X, nd->Center.Y));
			}
		}


		// Draw Trusted
		for (concurrent_hash_map<Hash, shared_ptr<Node>>::iterator _ts = GlobalNodes.begin(); _ts != GlobalNodes.end(); ++_ts)
		{
			shared_ptr<Node> ND;
			ND = _ts->second;
			NodeData* nd = &sim_nData[_ts->first];

			for (hash_map<Hash, shared_ptr<Node>>::iterator trusted = ND->TrustedNodes.begin(); trusted != ND->TrustedNodes.end(); ++trusted)
			{
				shared_ptr<Node> Trusted;
				Trusted = trusted->second;
				NodeData* TrustedLinkData = &sim_nData[Trusted->PublicKey];

				g->DrawLine(gcnew Pen(Color::LightBlue, 0.5F), Point(TrustedLinkData->Center.X, TrustedLinkData->Center.Y+1), Point(nd->Center.X, nd->Center.Y+1));
			}
		}

		for (concurrent_hash_map<Hash, shared_ptr<Node>>::iterator _ts = GlobalNodes.begin(); _ts != GlobalNodes.end(); ++_ts)
		{
			NodeData* nd = &sim_nData[_ts->first];
			std::string ss = _ts->first.ToString();

			shared_ptr<Node> ndd = _ts->second;

			g->FillEllipse(Brushes::LightBlue, System::Drawing::Rectangle(nd->Center.X - 10, nd->Center.Y - 10, 20, 20));

			String ^sss = "ID:" +
				StringUtils::stops(ss)->Substring(0, 8) +
				" Money:" + (int)(ndd->LocalMoney + ndd->Money());

			g->DrawString(sss, gcnew System::Drawing::Font("Consolas", 8, FontStyle::Regular), Brushes::DarkMagenta, Point(nd->Center.X, nd->Center.Y+15));
			g->DrawString("InCD:" + ndd->ledger->getCandidates().size() + " OutTX:" + ndd->OutTransactionCount +
				" InTX:" + ndd->InTransactionCount +
				" InPendingTX:" + ndd->PendingIncomingTransactions.unsafe_size(), gcnew System::Drawing::Font("Consolas", 8, FontStyle::Regular), Brushes::DarkGreen, Point(nd->Center.X, nd->Center.Y + 30));

		}
	}

	};
}



