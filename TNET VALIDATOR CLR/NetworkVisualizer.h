
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
#include "NodeController.h"
#include "Constants.h"
#include <hash_map>

extern hash_map<Hash, Node> sim_nodes;
extern vector<Point2> XY;

typedef struct NodeData
{
public: Point2 Corner;
public: Point2 Center;

		NodeData()
		{

		}
} NodeData;

extern hash_map<Hash, NodeData> nData;

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

			XY.push_back(Point2((int)(this->Width * (1.0 / 2.0)), (int)(this->Height * (1.0 / 2.0))));
			XY.push_back(Point2((int)(this->Width * (1.0 / 3.0)), (int)(this->Height * (1.0 / 3.0))));
			XY.push_back(Point2((int)(this->Width * (2.0 / 3.0)), (int)(this->Height * (2.0 / 3.0))));


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

		XY.clear();
		nData.clear();
		
		XY.push_back(Point2((int)(this->Width * (2.0 / 5.0)), (int)(this->Height * (1.0 / 5.0))));
		XY.push_back(Point2((int)(this->Width * (3.0 / 5.0)), (int)(this->Height * (1.0 / 5.0))));

		XY.push_back(Point2((int)(this->Width * (1.0 / 5.0)), (int)(this->Height * (2.0 / 5.0))));
		XY.push_back(Point2((int)(this->Width * (4.0 / 5.0)), (int)(this->Height * (2.0 / 5.0))));

		XY.push_back(Point2((int)(this->Width * (1.0 / 5.0)), (int)(this->Height * (3.0 / 5.0))));
		XY.push_back(Point2((int)(this->Width * (4.0 / 5.0)), (int)(this->Height * (3.0 / 5.0))));

		XY.push_back(Point2((int)(this->Width * (2.0 / 5.0)), (int)(this->Height * (4.0 / 5.0))));
		XY.push_back(Point2((int)(this->Width * (3.0 / 5.0)), (int)(this->Height * (4.0 / 5.0))));

	}

	public: void OnPaint(PaintEventArgs^ e) override
	{
		Graphics^ g = e->Graphics;

		g->SmoothingMode = System::Drawing::Drawing2D::SmoothingMode::AntiAlias;
		//g->TextRenderingHint = System::Drawing::Text::TextRenderingHint::AntiAlias;

		g->Clear(Color::GhostWhite);

		int i = 0;
		for (hash_map<Hash, Node>::iterator _ts = sim_nodes.begin(); _ts != sim_nodes.end(); ++_ts)
		{
			Node *ND;
			ND = &_ts->second;
			if (nData.count(ND->PublicKey) == 0)
			{
				NodeData d;
				Point2 s = Point2(XY[i].X - 10, XY[i].Y - 10);
				d.Center = s;
				d.Corner = XY[i];
				nData[ND->PublicKey] = d;
				i++;
			}
		}

		for (hash_map<Hash, Node>::iterator _ts = sim_nodes.begin(); _ts != sim_nodes.end(); ++_ts)
		{
			Node* ND;
			ND = &_ts->second;
			NodeData* nd = &nData[_ts->first];

			for (hash_map<Hash, Node>::iterator links = ND->Connections.begin(); links != ND->Connections.end(); ++links)
			{
				Node* Link;
				Link = &links->second;
				NodeData* LinkData = &nData[Link->PublicKey];

				g->DrawLine(gcnew Pen(Color::LightGray, 0.5F), Point(LinkData->Center.X, LinkData->Center.Y), Point(nd->Center.X, nd->Center.Y));
			}
		}

		for (hash_map<Hash, Node>::iterator _ts = sim_nodes.begin(); _ts != sim_nodes.end(); ++_ts)
		{
			NodeData* nd = &nData[_ts->first];
			std::string ss = _ts->first.ToString();

			g->FillEllipse(Brushes::LightBlue, System::Drawing::Rectangle(nd->Center.X - 10, nd->Center.Y - 10, 20, 20));

			String ^sss = "ID:" +
				StringUtils::stops(ss)->Substring(0, 8) +
				" Money:" +
				(int)_ts->second.Money();

			g->DrawString(sss, gcnew System::Drawing::Font("Consolas", 8, FontStyle::Regular), Brushes::DarkMagenta, Point(nd->Center.X, nd->Center.Y+15));
			g->DrawString("Candidates:" + _ts->second.ledger.getCandidates().size() + " Out TX:" + _ts->second.OutTransactionCount +
				" In TX:" + _ts->second.InCandidatesCount, gcnew System::Drawing::Font("Consolas", 8, FontStyle::Regular), Brushes::DarkGreen, Point(nd->Center.X, nd->Center.Y + 30));

		}
	}

	};
}



