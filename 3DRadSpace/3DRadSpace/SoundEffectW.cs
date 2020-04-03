﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using _3DRadSpaceDll;

namespace _3DRadSpace
{
	public partial class SoundEffectW : Form
	{
		public GameObject Result;
		public SoundEffectW()
		{
			InitializeComponent();
		}
		public SoundEffectW(SoundEffect sound)
		{
			InitializeComponent();
			textBox1.Text = sound.Name;
			textBox2.Text = sound.Resource;
			checkBox1.Checked = sound.Enabled;
			trackBar1.Value = (int)sound.Volume*100;
			trackBar2.Value = (int)(sound.Pitch * 100);
			trackBar3.Value = (int)(sound.Pan * 100);
			trackBar1_Scroll(null, null);
			trackBar2_Scroll(null, null);
			trackBar3_Scroll(null, null);
			switch(sound.SoundState)
			{
				case Microsoft.Xna.Framework.Audio.SoundState.Playing:
					{
						radioButton1.Checked = true;
						break;
					}
				case Microsoft.Xna.Framework.Audio.SoundState.Paused:
					{
						radioButton2.Checked = true;
						break;
					}
				case Microsoft.Xna.Framework.Audio.SoundState.Stopped:
					{
						radioButton3.Checked = true;
						break;
					}
				default: break;
			}
		}


		private void button3_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (!File.Exists("Content\\" + textBox2.Text + ".xnb"))
			{
				MessageBox.Show("The file: \n" + textBox2.Text + " doesn't exist.", "Resource file not found.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			Microsoft.Xna.Framework.Audio.SoundState sw = Microsoft.Xna.Framework.Audio.SoundState.Stopped;
			if (radioButton1.Checked == true) sw = Microsoft.Xna.Framework.Audio.SoundState.Playing;
			if (radioButton2.Checked == true) sw = Microsoft.Xna.Framework.Audio.SoundState.Paused;
			if (radioButton3.Checked == true) sw = Microsoft.Xna.Framework.Audio.SoundState.Stopped;
			DialogResult = DialogResult.OK;
			Result = new SoundEffect(textBox1.Text, checkBox1.Checked, textBox2.Text, trackBar1.Value)
			{
				Pitch = trackBar2.Value / 100f,
				Pan = trackBar3.Value / 100f,
				SoundState = sw
			};
			Close();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//open docs
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			//radioButton2.Checked = false;
			//radioButton3.Checked = false;
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			//radioButton1.Checked = false;
			//radioButton3.Checked = false;
		}

		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
		   //radioButton1.Checked = false;
		   //radioButton2.Checked = false;
		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			label4.Text = trackBar1.Value + "";
		}

		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			label5.Text = trackBar2.Value / 100f + "";
		}

		private void trackBar3_Scroll(object sender, EventArgs e)
		{
			label7.Text = trackBar3.Value / 100f + "";
		}

		private void SoundEffectW_Load(object sender, EventArgs e)
		{
			trackBar1_Scroll(null,null);
			trackBar2_Scroll(null,null);
			trackBar3_Scroll(null,null);
		}
	}
}
