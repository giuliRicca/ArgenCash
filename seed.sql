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
-- Create demo user (password: demo1234)
-- ---------------------------------------------------------------------------
INSERT INTO public."Users" ("Id", "FullName", "Email", "PasswordHash", "CreatedAt")
SELECT 'a75dccd9-b06c-4821-b198-68f6dde510fe', 'Demo User', 'demo@example.com', '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.VTtYWVJOg.7hLG', NOW()
WHERE NOT EXISTS (SELECT 1 FROM public."Users" WHERE "Id" = 'a75dccd9-b06c-4821-b198-68f6dde510fe');

-- ---------------------------------------------------------------------------
-- Categories (system categories seeded by EF migrations, adding demo custom ones)
-- ---------------------------------------------------------------------------

INSERT INTO public."Categories" ("Id", "Name", "Type", "IsSystem", "UserId", "CreatedAtUtc")
SELECT 'c0000001-0000-0000-0000-000000000001', 'Groceries', 'EXPENSE', false, 'a75dccd9-b06c-4821-b198-68f6dde510fe', NOW()
WHERE NOT EXISTS (SELECT 1 FROM public."Categories" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Groceries');

INSERT INTO public."Categories" ("Id", "Name", "Type", "IsSystem", "UserId", "CreatedAtUtc")
SELECT 'c0000001-0000-0000-0000-000000000002', 'Entertainment', 'EXPENSE', false, 'a75dccd9-b06c-4821-b198-68f6dde510fe', NOW()
WHERE NOT EXISTS (SELECT 1 FROM public."Categories" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Entertainment');

INSERT INTO public."Categories" ("Id", "Name", "Type", "IsSystem", "UserId", "CreatedAtUtc")
SELECT 'c0000001-0000-0000-0000-000000000003', 'Dividends', 'INCOME', false, 'a75dccd9-b06c-4821-b198-68f6dde510fe', NOW()
WHERE NOT EXISTS (SELECT 1 FROM public."Categories" WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Dividends');

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
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT '3762f63f-3f51-49ef-93f0-2d56ed0f1001', a."Id", 1250.00, 'USD', 'INCOME', 'Client payment', 1250.00, 1734651.50, NULL, '66666666-6666-6666-6666-666666666666', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Client payment');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT 'e9f82250-f6dc-4361-b5f0-f70913a71002', a."Id", 320.50, 'USD', 'INCOME', 'Platform payout', 320.50, 444765.14, NULL, '66666666-6666-6666-6666-666666666666', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Platform payout');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT '11a2b3c4-55d6-47e8-99f0-1a2b3c4d5e01', a."Id", 145.90, 'USD', 'EXPENSE', 'Software subscription', 145.90, 202445.48, NULL, '44444444-4444-4444-4444-444444444444', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Software subscription');

-- Broker ARS
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT 'a5eb88dd-a3eb-49bb-9e91-9a8d58df1003', a."Id", 850000.00, 'ARS', 'INCOME', 'Broker deposit', 612.51, 850000.00, NULL, '88888888-8888-8888-8888-888888888888', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Broker deposit');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT 'b0ec9d4a-fd2d-4e1d-a77f-644d58df1004', a."Id", 125000.00, 'ARS', 'INCOME', 'Stock sale proceeds', 90.07, 125000.00, NULL, '88888888-8888-8888-8888-888888888888', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Stock sale proceeds');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT '22b3c4d5-66e7-48f9-aaf1-2b3c4d5e6f02', a."Id", 210000.00, 'ARS', 'EXPENSE', 'Broker commission', 151.33, 210000.00, NULL, '44444444-4444-4444-4444-444444444444', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Broker commission');

-- Savings USD
INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT 'd78691d0-f5fc-446c-a7b7-8a84f9df1005', a."Id", 5000.00, 'USD', 'INCOME', 'Initial savings', 5000.00, 6938606.00, NULL, '88888888-8888-8888-8888-888888888888', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Initial savings');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT '836a34b2-7432-4d8d-b711-2e8af9df1006', a."Id", 275.25, 'USD', 'INCOME', 'Interest credit', 275.25, 381936.22, NULL, '88888888-8888-8888-8888-888888888888', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Interest credit');

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "TransactionType", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "CategoryId", "TransactionDate")
SELECT '33c4d5e6-77f8-49aa-bb22-3c4d5e6f7f03', a."Id", 625.40, 'USD', 'EXPENSE', 'Emergency transfer', 625.40, 867871.46, NULL, '55555555-5555-5555-5555-555555555555', NOW()
FROM public."Accounts" a WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (SELECT 1 FROM public."Transactions" WHERE "AccountId" = a."Id" AND "Description" = 'Emergency transfer');
