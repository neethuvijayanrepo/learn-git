using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.DemoMap
{
    public class Address
    {
        private Doc.DocRecord _source;
        private AddressOwner _type;
        public Address(Doc.DocRecord source, AddressOwner type = AddressOwner.Guarantor)
        {
            _source = source;
            _type = type;
            Refresh();
        }

        public void Refresh()
        {
            _Address1 = this[nameof(Address1)];
            _Address2 = this[nameof(Address2)];
            _City = this[nameof(City)];
            _State = this[nameof(State)];
            _Zip = this[nameof(Zip)];
        }
        string colMap(string col)
        {
            return _type.ToString("F") + col;
        }
        public string this[string field]
        {
            get { return _source[colMap(field)]; }
            set
            {
                _source[colMap(field)] = value;
            }
        }

        private string _Address1;
        public string Address1
        {
            get { return _Address1; }
            set
            {
                if (_Address1 != value)
                {
                    _Address1 = value;
                    this[nameof(Address1)] = value;
                }
            }
        }

        private string _Address2;
        public string Address2
        {
            get { return _Address2; }
            set
            {
                if (_Address2 != value)
                {
                    _Address2 = value;
                    this[nameof(Address2)] = value;
                }
            }
        }

        private string _City;
        public string City
        {
            get { return _City; }
            set
            {
                if (_City != value)
                {
                    _City = value;
                    this[nameof(City)] = value;
                }
            }
        }

        private string _State;
        public string State
        {
            get { return _State; }
            set
            {
                if (_State != value)
                {
                    _State = value;
                    this[nameof(State)] = value;
                }
            }
        }

        private string _Zip;
        public string Zip
        {
            get { return _Zip; }
            set
            {
                if (_Zip != value)
                {
                    _Zip = value;
                    this[nameof(Zip)] = value;
                }
            }
        }
    }
}
