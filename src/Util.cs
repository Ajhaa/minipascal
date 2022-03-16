using System.Collections.Generic;
using System;
using static TokenType;
public class Util
{
    public static List<byte> LEB128encode(int number)
    {
        var result = new List<byte>();

        while (true)
        {
            var b = (number & 0x7f);
            number >>= 7;

            if ((number == 0 && (b & 0x40) == 0) ||
                (number == -1 && (b & 0x40) != 0))
            {
                result.Add(Convert.ToByte(b));
                return result;
            }

            result.Add(Convert.ToByte(b | 0x80));
        }
    }

    // TODO move this elsewhere?
    public static byte OpToIntegerInstruction(TokenType operation)
    {
        switch (operation)
        {
            case PLUS:
                return 0x6a;
            case MINUS:
                return 0x6b;
            case STAR:
                return 0x6c;
            case SLASH:
                return 0x6d;
            case PERCENT:
                return 0x6f;
            case LESS:
                return 0x48;
            case LESS_EQ:
                return 0x4C;
            case GREATER:
                return 0x4A;
            case GREATER_EQ:
                return 0x4E;
            case EQUAL:
                return 0x46;
            case NOT_EQUAL:
                return 0x47;
            default:
                throw new Exception("not a operation" + operation);
        }
    }

    public static string OpToC(TokenType operation)
    {
        switch (operation)
        {
            case PLUS:
                return "+";
            case MINUS:
                return "-";
            case STAR:
                return "*";
            case SLASH:
                return "/";
            case PERCENT:
                return "%";
            case LESS:
                return "<";
            case LESS_EQ:
                return "<=";
            case GREATER:
                return ">";
            case GREATER_EQ:
                return ">=";
            case EQUAL:
                return "==";
            case NOT_EQUAL:
                return "!=";
            default:
                throw new Exception("not a operation" + operation);
        }
    }
}
