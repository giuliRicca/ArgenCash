-- ArgenCash demo seed data
--
-- Prerequisites:
-- 1. Run all EF Core migrations first:
--    dotnet ef database update --project ArgenCash.Infrastructure --startup-project ArgenCash.Api
-- 2. Ensure a user with Id 'a75dccd9-b06c-4821-b198-68f6dde510fe' exists in the Users table
--    (created via POST /api/auth/register or manually inserted).
--
-- Usage:
--    psql "host=localhost port=5432 dbname=ArgenCashDB user=postgres password=admin" -f seed.sql
--
-- This script is idempotent: running it multiple times will not duplicate rows.

-- ---------------------------------------------------------------------------
-- Verify user exists
-- ---------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM public."Users" WHERE "Id" = 'a75dccd9-b06c-4821-b198-68f6dde510fe'
    ) THEN
        RAISE EXCEPTION 'User a75dccd9-b06c-4821-b198-68f6dde510fe not found. Register a user first or adjust the Id below.';
    END IF;
END $$;

-- ---------------------------------------------------------------------------
-- Accounts
-- ---------------------------------------------------------------------------
INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT '4e1904bd-bb4b-4a61-90aa-1775c2817a01', 'Payoneer USD', 'USD', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (SELECT 1 FROM public."Accounts" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Payoneer USD');

INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT '0ca0f2ad-8041-487d-a796-5c4a2ec3bb02', 'Broker ARS', 'ARS', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (SELECT 1 FROM public."Accounts" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Broker ARS');

INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT 'f39c7d98-02fe-4428-9544-395cb4bc9c03', 'Savings USD', 'USD', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (SELECT 1 FROM public."Accounts" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Savings USD');

-- ---------------------------------------------------------------------------
-- Transactions (positive amounts with explicit INCOME / EXPENSE type)
-- ---------------------------------------------------------------------------

-- Payoneer USD
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '3762f63f-3f51-49ef-93f0-2d56ed0f1001', a."Id", 1250.00, 'USD', 'INCOME', 'Client payment', 1250.00, 1734651.50, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Client payment');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'e9f82250-f6dc-4361-b5f0-f70913a71002', a."Id", 320.50, 'USD', 'INCOME', 'Platform payout', 320.50, 444765.14, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Platform payout');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '11a2b3c4-55d6-47e8-99f0-1a2b3c4d5e01', a."Id", 145.90, 'USD', 'EXPENSE', 'Software subscription', 145.90, 202445.48, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Software subscription');

-- Broker ARS
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'a5eb88dd-a3eb-49bb-9e91-9a8d58df1003', a."Id", 850000.00, 'ARS', 'INCOME', 'Broker deposit', 612.51, 850000.00, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Broker deposit');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'b0ec9d4a-fd2d-4e1d-a77f-644d58df1004', a."Id", 125000.00, 'ARS', 'INCOME', 'Stock sale proceeds', 90.07, 125000.00, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Stock sale proceeds');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '22b3c4d5-66e7-48f9-aaf1-2b3c4d5e6f02', a."Id", 210000.00, 'ARS', 'EXPENSE', 'Broker commission', 151.33, 210000.00, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Broker commission');

-- Savings USD
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'd78691d0-f5fc-446c-a7b7-8a84f9df1005', a."Id", 5000.00, 'USD', 'INCOME', 'Initial savings', 5000.00, 6938606.00, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Initial savings');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '836a34b2-7432-4d8d-b711-2e8af9df1006', a."Id", 275.25, 'USD', 'INCOME', 'Interest credit', 275.25, 381936.22, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Interest credit');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '33c4d5e6-77f8-49ga-bbg2-3c4d5e6f7g03', a."Id", 625.40, 'USD', 'EXPENSE', 'Emergency transfer', 625.40, 867871.46, NULL, NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Emergency transfer');
