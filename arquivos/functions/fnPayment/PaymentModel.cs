using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fnPayment.Model
{
    internal class PaymentModel
    {
        public Guid id { get { return Guid.NewGuid(); } }
        public Guid IdPayment { get { return Guid.NewGuid(); } }
        public string nome { get; set; }
        public string email { get; set; }
        public string modelo { get; set; }
        public int ano { get; set; }
        public string tempoAluguel { get; set; }
        public DateTime data { get; set; }
        public string Status {  get; set; }
        public DateTime? DataAprovacao { get; set; }

    }
}
