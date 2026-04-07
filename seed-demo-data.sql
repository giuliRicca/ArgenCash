DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM public."Users"
        WHERE "Id" = 'a75dccd9-b06c-4821-b198-68f6dde510fe'
    ) THEN
        RAISE EXCEPTION 'User % not found.', 'a75dccd9-b06c-4821-b198-68f6dde510fe';
    END IF;
END $$;

INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT '4e1904bd-bb4b-4a61-90aa-1775c2817a01', 'Payoneer USD', 'USD', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (
    SELECT 1 FROM public."Accounts"
    WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Payoneer USD'
);

INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT '0ca0f2ad-8041-487d-a796-5c4a2ec3bb02', 'Broker ARS', 'ARS', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (
    SELECT 1 FROM public."Accounts"
    WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Broker ARS'
);

INSERT INTO public."Accounts" ("Id", "Name", "CurrencyCode", "CreatedAt", "UserId")
SELECT 'f39c7d98-02fe-4428-9544-395cb4bc9c03', 'Savings USD', 'USD', NOW(), 'a75dccd9-b06c-4821-b198-68f6dde510fe'
WHERE NOT EXISTS (
    SELECT 1 FROM public."Accounts"
    WHERE "UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND "Name" = 'Savings USD'
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '3762f63f-3f51-49ef-93f0-2d56ed0f1001', a."Id", 1250.00, 'USD', 'Client payment', 1250.00, 1734651.50, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Client payment' AND "Amount" = 1250.00
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'e9f82250-f6dc-4361-b5f0-f70913a71002', a."Id", 320.50, 'USD', 'Platform payout', 320.50, 444765.14, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Payoneer USD'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Platform payout' AND "Amount" = 320.50
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'a5eb88dd-a3eb-49bb-9e91-9a8d58df1003', a."Id", 850000.00, 'ARS', 'Broker deposit', 612.51, 850000.00, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Broker deposit' AND "Amount" = 850000.00
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'b0ec9d4a-fd2d-4e1d-a77f-644d58df1004', a."Id", 125000.00, 'ARS', 'Stock sale proceeds', 90.07, 125000.00, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Broker ARS'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Stock sale proceeds' AND "Amount" = 125000.00
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT 'd78691d0-f5fc-446c-a7b7-8a84f9df1005', a."Id", 5000.00, 'USD', 'Initial savings', 5000.00, 6938606.00, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Initial savings' AND "Amount" = 5000.00
);

INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Description", "ConvertedAmountUSD", "ConvertedAmountARS", "ExchangeRateId", "TransactionDate")
SELECT '836a34b2-7432-4d8d-b711-2e8af9df1006', a."Id", 275.25, 'USD', 'Interest credit', 275.25, 381936.22, NULL, NOW()
FROM public."Accounts" a
WHERE a."UserId" = 'a75dccd9-b06c-4821-b198-68f6dde510fe' AND a."Name" = 'Savings USD'
AND NOT EXISTS (
    SELECT 1 FROM public."Transactions"
    WHERE "AccountId" = a."Id" AND "Description" = 'Interest credit' AND "Amount" = 275.25
);
