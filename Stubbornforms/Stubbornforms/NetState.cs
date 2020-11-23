using PetriNetBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stubbornforms
{
    public class NetState
    {
        private static int counter = 0;

        public int[] States 
        {
            get;
            private set;
        }

        public int Id
        {
            get;
            private set;
        }
        public List<NetState> Neighbours
        {get;set;}

        static public void initCounter() {
            counter = 0;
        }

        public NetState(List<Place> places)
        {
            Id = counter++;
            Neighbours = new List<NetState>();

            States = new int[places.Count];

            for (int i = 0; i < States.Length; i++)
            {
                States[i] = places[i].TokenCount;
            }
        }
        public NetState(NetState netState, NetTransition trans)
        {
            Id = counter++;
            Neighbours = new List<NetState>();

            if (netState.fireable(trans))
            {
                States = new int[netState.States.Length];
                for (int i = 0; i < States.Length; i++)
                {
                    States[i] = netState.States[i] - trans.inEdges[i] + trans.outEdges[i];
                }
            }
            else {
                States = new int[netState.States.Length];
                for (int i = 0; i < States.Length; i++)
                {
                    States[i] = 0;
                }
            }
        }

        public bool fireable(NetTransition nt) {
            if (nt.inEdges.Length != States.Length)
                return false;

            for (int i = 0; i < States.Length; i++)
            {
                if ((States[i] - nt.inEdges[i]) < 0)
                    return false;
            }

            return true;
        }

        public void fire(NetTransition nt) {
            if (fireable(nt)) {
                for (int i = 0; i < States.Length; i++)
                {
                    States[i] = States[i] - nt.inEdges[i] + nt.outEdges[i]; 
                }
            }
        }

        public string toString() {
            string message = "";

            for (int i = 0; i < States.Length-1; i++)
            {
                message += States[i];
            }
            message += States[States.Length - 1];
            
            return message;
        }
        
    }
}
