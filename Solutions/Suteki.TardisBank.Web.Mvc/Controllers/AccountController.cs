namespace Suteki.TardisBank.Web.Mvc.Controllers
{
    using System;
    using System.Web.Mvc;

    using SharpArch.RavenDb.Web.Mvc;

    using Suteki.TardisBank.Domain;
    using Suteki.TardisBank.Tasks;
    using Suteki.TardisBank.Web.Mvc.Controllers.ViewModels;
    using Suteki.TardisBank.Web.Mvc.Utilities;
    
    public class AccountController : Controller
    {
        readonly IUserService userService;

        public AccountController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet, UnitOfWork]
        public ActionResult MakePayment(string id)
        {
            // id is the child's user name
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            var parent = this.userService.CurrentUser as Parent;
            var child = this.userService.GetUser(id) as Child;

            if (this.userService.AreNullOrNotRelated(parent, child)) return StatusCode.NotFound;

            return this.View("MakePayment", new MakePaymentViewModel
            {
                ChildId = child.Id,
                ChildName = child.Name,
                Description = "",
                Amount = 0M
            });
        }

        [HttpPost, UnitOfWork]
        public ActionResult MakePayment(MakePaymentViewModel makePaymentViewModel)
        {
            if (!this.ModelState.IsValid) return this.View("MakePayment", makePaymentViewModel);
            if (makePaymentViewModel == null)
            {
                throw new ArgumentNullException("makePaymentViewModel");
            }

            if (makePaymentViewModel.Amount == 0M)
            {
                this.ModelState.AddModelError("Amount", "A payment of zero? That's not nice.");
                return this.View("MakePayment", makePaymentViewModel);
            }

            var parent = this.userService.CurrentUser as Parent;
            var child = this.userService.GetUser(makePaymentViewModel.ChildId) as Child;

            if (this.userService.AreNullOrNotRelated(parent, child)) return StatusCode.NotFound;

            parent.MakePaymentTo(child, makePaymentViewModel.Amount, makePaymentViewModel.Description);

            return this.View("PaymentConfirmation", makePaymentViewModel);
        }


        [HttpGet, UnitOfWork]
        public ActionResult ParentView(string id)
        {
            var parent = this.userService.CurrentUser as Parent;
            var child = this.userService.GetUser(id) as Child;

            if (this.userService.AreNullOrNotRelated(parent, child)) return StatusCode.NotFound;

            return this.View("Summary", new AccountSummaryViewModel
            {
                Parent = parent,
                Child = child
            });
        }

        [HttpGet, UnitOfWork]
        public ActionResult ChildView()
        {
            var child = this.userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }
            return this.View("Summary", new AccountSummaryViewModel
            {
                Child = child
            });
        }

        [HttpGet, UnitOfWork]
        public ActionResult WithdrawCash()
        {
            var child = this.userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }

            return this.View("WithdrawCash", new WithdrawCashViewModel
            {
                Amount = 0M,
                Description = ""
            });
        }

        [HttpPost, UnitOfWork]
        public ActionResult WithdrawCash(WithdrawCashViewModel withdrawCashViewModel)
        {
            if (!this.ModelState.IsValid) return this.View("WithdrawCash", withdrawCashViewModel);
            if (withdrawCashViewModel == null)
            {
                throw new ArgumentNullException("withdrawCashViewModel");
            }

            if (withdrawCashViewModel.Amount == 0M)
            {
                this.ModelState.AddModelError("Amount", "There's no point in asking for zero cash.");
                return this.View("WithdrawCash", withdrawCashViewModel);
            }

            var child = this.userService.CurrentUser as Child;
            if (child == null)
            {
                return StatusCode.NotFound;
            }
            var parent = this.userService.GetUser(child.ParentId) as Parent;
            if (parent == null)
            {
                throw new TardisBankException("Parent with id '{0}' not found", child.ParentId);
            }

            try
            {
                child.WithdrawCashFromParent(
                    parent, 
                    withdrawCashViewModel.Amount, 
                    withdrawCashViewModel.Description);
            }
            catch (CashWithdrawException cashWithdrawException)
            {
                this.ModelState.AddModelError("Amount", cashWithdrawException.Message);
                return this.View("WithdrawCash", withdrawCashViewModel);
            }

            return this.View("WithdrawCashConfirm", withdrawCashViewModel);
        }

        [HttpGet, UnitOfWork]
        public ActionResult WithdrawCashForChild(string id)
        {
            // id is the child's user name
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            var parent = this.userService.CurrentUser as Parent;
            var child = this.userService.GetUser(id) as Child;

            if (this.userService.AreNullOrNotRelated(parent, child)) return StatusCode.NotFound;

            return this.View(new WithdrawCashForChildViewModel
            {
                ChildId = child.Id,
                ChildName = child.Name,
                Description = "",
                Amount = 0M
            });
        }

        [HttpPost, UnitOfWork]
        public ActionResult WithdrawCashForChild(WithdrawCashForChildViewModel withdrawCashForChildViewModel)
        {
            if (!this.ModelState.IsValid) return this.View(withdrawCashForChildViewModel);
            if (withdrawCashForChildViewModel == null)
            {
                throw new ArgumentNullException("withdrawCashForChildViewModel");
            }

            if (withdrawCashForChildViewModel.Amount == 0M)
            {
                this.ModelState.AddModelError("Amount", "0.00 is not a valid amount.");
                return this.View(withdrawCashForChildViewModel);
            }

            var child = this.userService.GetUser(withdrawCashForChildViewModel.ChildId) as Child;
            var parent = this.userService.CurrentUser as Parent;
            if (this.userService.AreNullOrNotRelated(parent, child)) return StatusCode.NotFound;

            try
            {
                child.AcceptCashFromParent(
                    parent,
                    withdrawCashForChildViewModel.Amount,
                    withdrawCashForChildViewModel.Description);
            }
            catch (CashWithdrawException cashWithdrawException)
            {
                this.ModelState.AddModelError("Amount", cashWithdrawException.Message);
                return this.View(withdrawCashForChildViewModel);
            }

            return this.View("WithdrawCashForChildConfirm", withdrawCashForChildViewModel);            
        }
    }
}