using System;
using System.Text.RegularExpressions;
using DSCons.Revit.Starter.Core.Models;

namespace DSCons.Revit.Starter.Core.Validation;

public static class ViewTemplateRuleEngine
{
    private static readonly Regex UserRegex = new Regex(@"User\s*([0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static ViewTemplateRuleResult AnalyzeViewName(string? viewName)
    {
        var result = new ViewTemplateRuleResult
        {
            ViewName = viewName ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(viewName))
        {
            return result;
        }

        string trimmedName = viewName!.Trim();

        // 1. Nhánh WORKING (chứa mẫu User1 .. User5)
        Match userMatch = UserRegex.Match(trimmedName);
        if (userMatch.Success)
        {
            result.Scope = "WORKING";
            if (int.TryParse(userMatch.Groups[1].Value, out int uNum))
            {
                result.UserNumber = uNum.ToString("D2");
                result.TargetFilterName = $"DSC_SS_USER {result.UserNumber}";
            }

            // Mặt cắt
            if (ContainsToken(trimmedName, "_SS_Hor") || ContainsToken(trimmedName, "_SS_Ver") ||
                ContainsToken(trimmedName, "_Hoz") || ContainsToken(trimmedName, "_Ver") ||
                ContainsToken(trimmedName, "Hor") || ContainsToken(trimmedName, "Ver") ||
                ContainsToken(trimmedName, "Section"))
            {
                result.IsMatched = true;
                result.Discipline = "Mặt cắt (Section)";
                result.SuggestedTemplate = "DSC_W_C_SS";
                return result;
            }

            // 3D View
            if (ContainsToken(trimmedName, "3D"))
            {
                result.IsMatched = true;
                result.Discipline = "3D Tầng (3D View)";
                result.SuggestedTemplate = "DSC_W_C_3D";
                return result;
            }

            // Mặt bằng theo 13 hệ kỹ thuật
            if (ContainsToken(trimmedName, "_CB") || EndsWithToken(trimmedName, "CB") ||
                ContainsToken(trimmedName, "Combine"))
            {
                result.IsMatched = true;
                result.Discipline = "CB (Combine)";
                result.SuggestedTemplate = "DSC_W_C_FP";
            }
            else if (ContainsToken(trimmedName, "_MV") || EndsWithToken(trimmedName, "MV") ||
                     ContainsToken(trimmedName, "_ME") || EndsWithToken(trimmedName, "ME") ||
                     ContainsToken(trimmedName, "Duct"))
            {
                result.IsMatched = true;
                result.Discipline = "MV/ME (Ống gió)";
                result.SuggestedTemplate = "DSC_W_M_FP (Duct)";
            }
            else if (ContainsToken(trimmedName, "_MP") || EndsWithToken(trimmedName, "MP") ||
                     ContainsToken(trimmedName, "Pipe") || ContainsToken(trimmedName, "Chiller"))
            {
                result.IsMatched = true;
                result.Discipline = "MP (Nước lạnh/Gas/Ngưng)";
                result.SuggestedTemplate = "DSC_W_M_FP (Pipe)";
            }
            else if (ContainsToken(trimmedName, "_FI") || EndsWithToken(trimmedName, "FI") ||
                     ContainsToken(trimmedName, "_FF") || EndsWithToken(trimmedName, "FF") ||
                     ContainsToken(trimmedName, "Chữa cháy"))
            {
                result.IsMatched = true;
                result.Discipline = "FI (Chữa cháy nước)";
                result.SuggestedTemplate = "DSC_W_F_FP";
            }
            else if (ContainsToken(trimmedName, "_HS") || EndsWithToken(trimmedName, "HS") ||
                     ContainsToken(trimmedName, "_SP") || EndsWithToken(trimmedName, "SP") ||
                     ContainsToken(trimmedName, "Cấp nước"))
            {
                result.IsMatched = true;
                result.Discipline = "HS (Cấp nước sinh hoạt)";
                result.SuggestedTemplate = "DSC_W_P_FP (Supply)";
            }
            else if (ContainsToken(trimmedName, "_HD") || EndsWithToken(trimmedName, "HD") ||
                     ContainsToken(trimmedName, "_DP") || EndsWithToken(trimmedName, "DP") ||
                     ContainsToken(trimmedName, "Thoát nước"))
            {
                result.IsMatched = true;
                result.Discipline = "HD (Thoát nước)";
                result.SuggestedTemplate = "DSC_W_P_FP (Drainage)";
            }
            else if (ContainsToken(trimmedName, "_EL") || EndsWithToken(trimmedName, "EL") ||
                     ContainsToken(trimmedName, "_EG") || EndsWithToken(trimmedName, "EG") ||
                     ContainsToken(trimmedName, "_EP") || EndsWithToken(trimmedName, "EP") ||
                     ContainsToken(trimmedName, "_EU") || EndsWithToken(trimmedName, "EU") ||
                     ContainsToken(trimmedName, "_FA") || EndsWithToken(trimmedName, "FA") ||
                     ContainsToken(trimmedName, "_ET") || EndsWithToken(trimmedName, "ET") ||
                     ContainsToken(trimmedName, "_E") || EndsWithToken(trimmedName, "E") ||
                     ContainsToken(trimmedName, "Điện"))
            {
                result.IsMatched = true;
                result.Discipline = "E (Điện / Báo cháy / Mạng)";
                result.SuggestedTemplate = "DSC_W_E_FP";
            }

            return result;
        }

        // 2. Nhánh EXPORT (Bản vẽ Shop / Dalux / Navisworks)
        string upper = trimmedName.ToUpperInvariant();
        if (upper.Contains("BV ") || upper.Contains("MẶT BẰNG") || upper.Contains("MAT BANG") ||
            upper.Contains("DALUX") || upper.Contains("NAVIS") || upper.Contains("SHOP"))
        {
            result.Scope = "EXPORT";

            if (upper.Contains("DALUX"))
            {
                result.IsMatched = true;
                result.Discipline = "3D Dalux";
                result.SuggestedTemplate = "DSC_E_C_3D (Dalux)";
            }
            else if (upper.Contains("NAVIS"))
            {
                result.IsMatched = true;
                result.Discipline = "3D Navisworks";
                result.SuggestedTemplate = "DSC_E_C_3D (Naviswork)";
            }
            else if (upper.Contains("COMBINE"))
            {
                result.IsMatched = true;
                result.Discipline = "Combine";
                result.SuggestedTemplate = "DSC_E_C_FP";
            }
            else if (upper.Contains("PCCC") || upper.Contains("CHUA CHAY") || upper.Contains("FF"))
            {
                result.IsMatched = true;
                result.Discipline = "Chữa cháy (FF)";
                result.SuggestedTemplate = "DSC_E_F_FP";
            }
            else if (upper.Contains("THONG GIO") || upper.Contains("DUCT") || upper.Contains("HUT KHOI"))
            {
                result.IsMatched = true;
                result.Discipline = "Ống gió (Duct)";
                result.SuggestedTemplate = "DSC_E_M_FP (Duct)";
            }
            else if (upper.Contains("ONG GAS") || upper.Contains("NUOC NGUNG") || upper.Contains("CHILLER") || upper.Contains("PIPE"))
            {
                result.IsMatched = true;
                result.Discipline = "Ống máy lạnh (Pipe)";
                result.SuggestedTemplate = "DSC_E_M_FP (Pipe)";
            }
            else if (upper.Contains("CAP NUOC") || upper.Contains("WATER SUPPLY"))
            {
                result.IsMatched = true;
                result.Discipline = "Cấp nước (Supply)";
                result.SuggestedTemplate = "DSC_E_P_FP (Supply)";
            }
            else if (upper.Contains("THOAT NUOC") || upper.Contains("DRAINAGE"))
            {
                result.IsMatched = true;
                result.Discipline = "Thoát nước (Drainage)";
                result.SuggestedTemplate = "DSC_E_P_FP (Drainage)";
            }
            else if (upper.Contains("DIEN") || upper.Contains("LIGHTING") || upper.Contains("POWER"))
            {
                result.IsMatched = true;
                result.Discipline = "Điện (Electrical)";
                result.SuggestedTemplate = "DSC_E_E_FP";
            }
            else if (upper.Contains("SECTION") || upper.Contains("MAT CAT"))
            {
                result.IsMatched = true;
                result.Discipline = "Mặt cắt (Section)";
                result.SuggestedTemplate = "DSC_W_C_SS";
            }

            return result;
        }

        return result;
    }

    /// <summary>
    /// Tra cứu Template làm việc nhanh theo mã phân hệ MEP (CB, MV, MP, FI, HS, HD, EL, ...)
    /// </summary>
    public static string GetSuggestedTemplateForSystem(string sysCode)
    {
        if (string.IsNullOrWhiteSpace(sysCode)) return "DSC_W_C_FP";

        string code = sysCode.Trim().ToUpperInvariant();
        return code switch
        {
            "CB" => "DSC_W_C_FP",
            "MV" => "DSC_W_M_FP (Duct)",
            "ME" => "DSC_W_M_FP (Duct)",
            "MP" => "DSC_W_M_FP (Pipe)",
            "FI" => "DSC_W_F_FP",
            "HS" => "DSC_W_P_FP (Supply)",
            "HD" => "DSC_W_P_FP (Drainage)",
            "EL" or "EG" or "EP" or "EU" or "FA" or "ET" => "DSC_W_E_FP",
            _ => "DSC_W_C_FP"
        };
    }

    private static bool ContainsToken(string source, string token)
    {
        return source.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool EndsWithToken(string source, string token)
    {
        return source.EndsWith(token, StringComparison.OrdinalIgnoreCase);
    }
}
