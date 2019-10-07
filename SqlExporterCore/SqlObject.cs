using System;
using System.Collections.Generic;

namespace SqlExporterCore
{
    public class SqlObject
    {
        public string Tipo { get; set; }
        public string Proprietario { get; set; }
        public string Nome { get; set; }
        public DateTime DataModificacao { get; set; }

        public override string ToString()
        {
            
            return $"[Tipo={Tipo}] [Proprietario={Proprietario}] [Nome={Nome}] [DataModificacao={DataModificacao.ToShortDateString()} {DataModificacao.ToShortTimeString()}]";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SqlObject))
                return false;

            var sqlObj = obj as SqlObject;
            return Tipo.Equals(sqlObj.Tipo)
                && Proprietario.Equals(sqlObj.Proprietario)
                && Nome.Equals(sqlObj.Nome)
                && DataModificacao.Equals(sqlObj.DataModificacao);
        }

        public override int GetHashCode()
        {
            var hashCode = HashCodeConst.HASH_BASE;
            hashCode = hashCode * HashCodeConst.HASH_FATOR + EqualityComparer<string>.Default.GetHashCode(Tipo);
            hashCode = hashCode * HashCodeConst.HASH_FATOR + EqualityComparer<string>.Default.GetHashCode(Proprietario);
            hashCode = hashCode * HashCodeConst.HASH_FATOR + EqualityComparer<string>.Default.GetHashCode(Nome);
            return hashCode;
        }
    }
}
