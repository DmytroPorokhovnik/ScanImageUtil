using System;

namespace ScanImageUtil.Back.Exceptions
{
    class RecognizedWrongSerialNumberException: Exception
    {
        public RecognizedWrongSerialNumberException(string message): base(message)
        {

        }
    }
}
