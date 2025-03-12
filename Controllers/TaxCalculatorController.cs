using Microsoft.AspNetCore.Mvc;

public class TaxCalculatorController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CalculateTax(decimal income, bool isSelfEmployed, bool hasStudentLoan)
    {
        var result = CalculateIncomeTax(income, isSelfEmployed, hasStudentLoan);

        ViewBag.Income = income;
        ViewBag.TaxOwed = result.TaxOwed;
        ViewBag.NIContribution = result.NIContribution;
        ViewBag.StudentLoan = result.StudentLoan;
        ViewBag.NetIncome = result.NetIncome;

        return View("Index");
    }

    private TaxResult CalculateIncomeTax(decimal income, bool isSelfEmployed, bool hasStudentLoan)
    {
        decimal personalAllowance = 12570;
        decimal taxOwed = 0, niContribution = 0, studentLoan = 0;

        // Reduce Personal Allowance above £100,000
        if (income > 100000)
        {
            personalAllowance -= (income - 100000) / 2;
            personalAllowance = Math.Max(personalAllowance, 0);
        }

        decimal taxableIncome = Math.Max(income - personalAllowance, 0);

        // Apply UK Tax Bands (2024-25)
        if (taxableIncome <= 37700)
        {
            taxOwed = taxableIncome * 0.20m;
        }
        else if (taxableIncome <= 125140)
        {
            taxOwed = (37700 * 0.20m) + ((taxableIncome - 37700) * 0.40m);
        }
        else
        {
            taxOwed = (37700 * 0.20m) + ((125140 - 37700) * 0.40m) + ((taxableIncome - 125140) * 0.45m);
        }

        // National Insurance (NI) Contributions
        if (isSelfEmployed)
        {
            niContribution = (income - 12570) * 0.09m; // Approximate rate for self-employed
        }
        else
        {
            if (income > 12570)
                niContribution = (Math.Min(income, 50270) - 12570) * 0.10m; // Standard NI Rate
            if (income > 50270)
                niContribution += (income - 50270) * 0.02m;
        }

        // Student Loan Repayments (Plan 2 & Plan 5)
        if (hasStudentLoan && income > 27660)
        {
            studentLoan = (income - 27660) * 0.09m;
        }

        decimal netIncome = income - taxOwed - niContribution - studentLoan;

        return new TaxResult
        {
            TaxOwed = taxOwed,
            NIContribution = niContribution,
            StudentLoan = studentLoan,
            NetIncome = netIncome
        };
    }
}

// Helper Class to Store Results
public class TaxResult
{
    public decimal TaxOwed { get; set; }
    public decimal NIContribution { get; set; }
    public decimal StudentLoan { get; set; }
    public decimal NetIncome { get; set; }
}
