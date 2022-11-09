DO $$
DECLARE limite integer;
DECLARE	offsete integer;
BEGIN
    SELECT 50 INTO limite;
    SELECT 0 INTO offsete;

    DROP TABLE IF EXISTS tmp_table;
    CREATE TABLE tmp_table AS
    SELECT   id
                        , userid
                        , matricula
                        , cargo
                        , ativo
                        , datacadastro
                        , dataatualizacao
                        , primeironome
                        , sobrenome
                        , datanascimento as date
                        , enderecoemail
                        , (SELECT count(id) FROM public.funcionarios) as total
                        FROM public.funcionarios
                        ORDER BY datacadastro
                        LIMIT limite
                        OFFSET offsete;
END $$;

SELECT * FROM tmp_table;