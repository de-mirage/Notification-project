using Orleans;
using Orleans.Concurrency;
using NotificationService.Models;
using NotificationService.Models.IGrains;

namespace ComputingService.Grains
{
    [Reentrant]
    public class ComputationalGrain : Grain, IComputationalGrain
    {
        private ComputationalResult _result = new ComputationalResult();
        private bool _isCompleted = false;

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var grainId = this.GetPrimaryKey();
            _result.TaskId = grainId.ToString();
            _result.Status = "Pending";
            _result.CreatedAt = DateTime.UtcNow;

            return base.OnActivateAsync(cancellationToken);
        }

        public async Task<ComputationalResult> ExecuteTaskAsync(ComputationalTask task)
        {
            _result.TaskId = task.TaskId;
            _result.Status = "Processing";
            
            try
            {
                // Perform the computation based on the operation type
                object? result = null;
                
                switch (task.Operation.ToLower())
                {
                    case "add":
                        if (task.Parameters.Length >= 2 && 
                            double.TryParse(task.Parameters[0].ToString(), out double a) &&
                            double.TryParse(task.Parameters[1].ToString(), out double b))
                        {
                            result = a + b;
                        }
                        break;
                        
                    case "multiply":
                        if (task.Parameters.Length >= 2 && 
                            double.TryParse(task.Parameters[0].ToString(), out double x) &&
                            double.TryParse(task.Parameters[1].ToString(), out double y))
                        {
                            result = x * y;
                        }
                        break;
                        
                    case "fibonacci":
                        if (task.Parameters.Length >= 1 && 
                            int.TryParse(task.Parameters[0].ToString(), out int n))
                        {
                            result = CalculateFibonacci(n);
                        }
                        break;
                        
                    case "factorial":
                        if (task.Parameters.Length >= 1 && 
                            int.TryParse(task.Parameters[0].ToString(), out int num))
                        {
                            result = CalculateFactorial(num);
                        }
                        break;
                        
                    case "sort":
                        if (task.Parameters.Length >= 1 && task.Parameters[0] is Array array)
                        {
                            var list = array.Cast<object>().ToList();
                            list.Sort();
                            result = list.ToArray();
                        }
                        break;
                        
                    default:
                        throw new NotSupportedException($"Operation '{task.Operation}' is not supported");
                }

                _result.Result = result;
                _result.Status = "Completed";
                _result.CompletedAt = DateTime.UtcNow;
                _isCompleted = true;
            }
            catch (Exception ex)
            {
                _result.Status = "Error";
                _result.ErrorMessage = ex.Message;
                _result.CompletedAt = DateTime.UtcNow;
            }

            return _result;
        }

        public Task<ComputationalResult> GetResultAsync()
        {
            return Task.FromResult(_result);
        }

        public Task<bool> IsCompletedAsync()
        {
            return Task.FromResult(_isCompleted);
        }

        private long CalculateFactorial(int n)
        {
            if (n < 0) throw new ArgumentException("Factorial is not defined for negative numbers");
            if (n == 0 || n == 1) return 1;
            
            long result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private long CalculateFibonacci(int n)
        {
            if (n < 0) throw new ArgumentException("Fibonacci is not defined for negative numbers");
            if (n == 0) return 0;
            if (n == 1) return 1;
            
            long prev = 0, curr = 1;
            for (int i = 2; i <= n; i++)
            {
                long next = prev + curr;
                prev = curr;
                curr = next;
            }
            return curr;
        }
    }
}
