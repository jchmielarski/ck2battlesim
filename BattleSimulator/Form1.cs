using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace BattleSimulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public const double TROOP_KILL_FACTOR = 0.015;
        public class Unit
        {
            protected double skirmish_attack;
            protected double skirmish_defense;
            protected double melee_attack;
            protected double melee_defense;
            protected double morale;
            protected int prevsize;
            protected int size;

            public Unit(int size)
            {
                this.size = size;
                prevsize = size;
            }

            public double GetMorale()
            {
                return morale * size;
            }

            public double GetAttack(int phase)
            {
                double attack;
                switch(phase)
                {
                    case 0:
                        attack = prevsize * skirmish_attack;
                        return attack;
                    case 1:
                        attack = prevsize * melee_attack;
                        return attack;
                    default:
                        return 0;
                }
            }

            public double GetDefense(int phase)
            {
                switch (phase)
                {
                    case 0:
                        return prevsize * skirmish_defense;
                    case 1:
                        return prevsize * melee_defense;
                    default:
                        return 0;
                }
            }

            public void CalculateLosses(int phase, double enemy_attack)
            {
                prevsize = size;
                double def = GetDefense(phase);
                size -= (int)(TROOP_KILL_FACTOR *size* (enemy_attack / def));
                if (size < 0)
                    size = 0;
            }

            public int GetSize()
            {
                return size;
            }
        }

        public class Infantry : Unit
        {

            public Infantry(int size) : base(size)
            {
                skirmish_attack = 0.25;
                skirmish_defense = 0.5;
                melee_attack = 1;
                melee_defense = 1;
                morale = 1;
            }
        }

        public class Cavalry : Unit
        {
            

            public Cavalry(int size) : base(size)
            {
                skirmish_attack = 0.5;
                skirmish_defense = 0.5;
                melee_attack = 3;
                melee_defense = 1.5;
                morale = 2;
            }
        }

        public class Archer : Unit
        {

            public Archer(int size) : base(size)
            {
                skirmish_attack = 3;
                skirmish_defense = 0.5;
                melee_attack = 0.25;
                melee_defense = 0.25;
                morale = 0.75;
            }
        }

        public class Flank
        {
            public Infantry inf;
            public Cavalry cav;
            public Archer arch;
            public double max_morale;
            public double morale;
            public double morale_loss = 0;

            public Flank(int inf, int cav, int arch)
            {
                this.inf = new Infantry(inf);
                this.cav = new Cavalry(cav);
                this.arch = new Archer(arch);
                this.max_morale = this.inf.GetMorale() + this.cav.GetMorale() + this.arch.GetMorale();
                this.morale = max_morale;
            }

            public int GetSize()
            {
                return inf.GetSize() + cav.GetSize() + arch.GetSize();
            }
            
            public double GetMorale()
            {
                return inf.GetMorale() + cav.GetMorale() + arch.GetMorale();
            }

            public double GetAttack(int phase)
            {
                double attack = inf.GetAttack(phase) + cav.GetAttack(phase) + arch.GetAttack(phase);
                return attack;
            }

            public void CalculateLosses(int phase, double enemy_attack)
            {
                int prevsize = this.GetSize();
                inf.CalculateLosses(phase, enemy_attack);
                cav.CalculateLosses(phase, enemy_attack);
                arch.CalculateLosses(phase, enemy_attack);
                morale_loss += 3 * (prevsize - this.GetSize());
                morale = GetMorale() - morale_loss;
            }
        }

        public class Army
        {
            public Flank left;
            public Flank centre;
            public Flank right;
            public double morale;

            public Army(int[] v)
            {
                left = new Flank(v[0], v[3], v[6]);
                centre = new Flank(v[1], v[4], v[7]);
                right = new Flank(v[2], v[5], v[8]);
                morale = this.GetMorale();
            }

            public double GetMorale()
            {
                return left.GetMorale() + centre.GetMorale() + right.GetMorale();
            }

            public void CalculatePhase(Army a, int phase)
            {
                left.CalculateLosses(phase, a.left.GetAttack(phase));
                right.CalculateLosses(phase, a.right.GetAttack(phase));
                centre.CalculateLosses(phase, a.centre.GetAttack(phase));
            }
        }

        public void updateView(Army a)
        {
            IL0.Text = a.left.inf.GetSize().ToString();
            IC0.Text = a.centre.inf.GetSize().ToString();
            IR0.Text = a.right.inf.GetSize().ToString();
            CL0.Text = a.left.cav.GetSize().ToString();
            CC0.Text = a.centre.cav.GetSize().ToString();
            CR0.Text = a.right.cav.GetSize().ToString();
            AL0.Text = a.left.arch.GetSize().ToString();
            AC0.Text = a.centre.arch.GetSize().ToString();
            AR0.Text = a.right.arch.GetSize().ToString();
            L0.Text = a.left.GetSize().ToString();
            C0.Text = a.centre.GetSize().ToString();
            R0.Text = a.right.GetSize().ToString();
        }

        public Army a0;
        public Army a1;
        public int phase = 0;

        public void Setup()
        {
            //initialize armies
            int[] v0 = new int[9];
            v0[0] = (int)Inf0L.Value;
            v0[1] = (int)Inf0C.Value;
            v0[2] = (int)Inf0R.Value;
            v0[3] = (int)Cav0L.Value;
            v0[4] = (int)Cav0C.Value;
            v0[5] = (int)Cav0R.Value;
            v0[6] = (int)Arch0L.Value;
            v0[7] = (int)Arch0C.Value;
            v0[8] = (int)Arch0R.Value;
            a0 = new Army(v0);
            v0[0] = (int)Inf1L.Value;
            v0[1] = (int)Inf1C.Value;
            v0[2] = (int)Inf1R.Value;
            v0[3] = (int)Cav1L.Value;
            v0[4] = (int)Cav1C.Value;
            v0[5] = (int)Cav1R.Value;
            v0[6] = (int)Arch1L.Value;
            v0[7] = (int)Arch1C.Value;
            v0[8] = (int)Arch1R.Value;
            a1 = new Army(v0);
            updateView(a0);
            //updateView(a1);
        }

        public void Battle(Army a0, Army a1, int phase)
        {
            a0.CalculatePhase(a1, phase);
            a1.CalculatePhase(a0, phase);
            updateView(a0);
        }

        private void PrepareButton_Click(object sender, EventArgs e)
        {
            Setup();
            FIghtTurn.Enabled = true;
        }

        private void FIghtTurn_Click(object sender, EventArgs e)
        {
            Battle(a0, a1, phase%2);
            phase++;
        }

        private void FightBattle_Click(object sender, EventArgs e)
        {
            bool win0=false, win1=false;
            while(win0==false&&win1==false)
            {
                Battle(a0, a1, phase % 2);
                phase++;
                if (a0.GetMorale() / a0.morale < 0.2)
                    win1 = true;
                if (a1.GetMorale() / a1.morale < 0.2)
                    win0 = true;
            }
        }
    }

    
}