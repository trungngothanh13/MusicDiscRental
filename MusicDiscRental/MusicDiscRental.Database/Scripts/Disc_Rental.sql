-- Create Discs table
CREATE TABLE Discs (
    disc_id NUMBER PRIMARY KEY,
    title VARCHAR2(100) NOT NULL,
    artist VARCHAR2(100) NOT NULL,
    genre VARCHAR2(50),
    total_stock NUMBER DEFAULT 1 NOT NULL,
    available_stock NUMBER DEFAULT 1 NOT NULL,
    CONSTRAINT chk_stock CHECK (available_stock <= total_stock)
);

-- Create Members table
CREATE TABLE Members (
    member_id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    phone_number VARCHAR2(15),
    join_date DATE DEFAULT SYSDATE NOT NULL
);

-- Create Rentals table
CREATE TABLE Rentals (
    rental_id NUMBER PRIMARY KEY,
    disc_id NUMBER NOT NULL,
    member_id NUMBER NOT NULL,
    rental_date DATE DEFAULT SYSDATE NOT NULL,
    return_date DATE,
    CONSTRAINT fk_disc FOREIGN KEY (disc_id) REFERENCES Discs(disc_id),
    CONSTRAINT fk_member FOREIGN KEY (member_id) REFERENCES Members(member_id)
);

-- Create sequences for generating IDs
CREATE SEQUENCE disc_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE member_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE rental_seq START WITH 1 INCREMENT BY 1;

-- Check the structure of the tables
DESCRIBE Discs;
DESCRIBE Members;
DESCRIBE Rentals;

CREATE OR REPLACE PROCEDURE RentDisc(
    p_member_id IN NUMBER,
    p_disc_id IN NUMBER
) AS
    v_available NUMBER;
    v_member_exists NUMBER;
    v_active_rentals NUMBER;
    e_no_stock EXCEPTION;
    e_invalid_member EXCEPTION;
    e_rental_limit EXCEPTION;
    RENTAL_LIMIT CONSTANT NUMBER := 5; -- Maximum number of active rentals per member
BEGIN
    -- Check if member exists
    SELECT COUNT(*) INTO v_member_exists FROM Members WHERE member_id = p_member_id;
    IF v_member_exists = 0 THEN
        RAISE e_invalid_member;
    END IF;
    
    -- Check if disc is available
    SELECT available_stock INTO v_available FROM Discs WHERE disc_id = p_disc_id;
    IF v_available <= 0 THEN
        RAISE e_no_stock;
    END IF;
    
    -- Check if member has reached rental limit
    SELECT COUNT(*) INTO v_active_rentals 
    FROM Rentals 
    WHERE member_id = p_member_id AND return_date IS NULL;
    
    IF v_active_rentals >= RENTAL_LIMIT THEN
        RAISE e_rental_limit;
    END IF;
    
    -- Create rental record
    INSERT INTO Rentals (rental_id, disc_id, member_id, rental_date, return_date)
    VALUES (rental_seq.NEXTVAL, p_disc_id, p_member_id, SYSDATE, NULL);
    
    -- Update available stock
    UPDATE Discs SET available_stock = available_stock - 1
    WHERE disc_id = p_disc_id;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Disc rented successfully.');
    
EXCEPTION
    WHEN e_no_stock THEN
        DBMS_OUTPUT.PUT_LINE('Error: Disc is not available for rent.');
        ROLLBACK;
    WHEN e_invalid_member THEN
        DBMS_OUTPUT.PUT_LINE('Error: Invalid member ID.');
        ROLLBACK;
    WHEN e_rental_limit THEN
        DBMS_OUTPUT.PUT_LINE('Error: Member has reached the maximum rental limit.');
        ROLLBACK;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        ROLLBACK;
END RentDisc;
/

CREATE OR REPLACE PROCEDURE ReturnDisc(
    p_rental_id IN NUMBER
) AS
    v_disc_id NUMBER;
    v_already_returned EXCEPTION;
    v_rental_not_found EXCEPTION;
BEGIN
    -- Check if rental exists and get disc_id
    BEGIN
        SELECT disc_id INTO v_disc_id 
        FROM Rentals 
        WHERE rental_id = p_rental_id;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE v_rental_not_found;
    END;
    
    -- Check if already returned
    DECLARE
        v_return_date DATE;
    BEGIN
        SELECT return_date INTO v_return_date 
        FROM Rentals 
        WHERE rental_id = p_rental_id;
        
        IF v_return_date IS NOT NULL THEN
            RAISE v_already_returned;
        END IF;
    END;
    
    -- Update return date
    UPDATE Rentals 
    SET return_date = SYSDATE
    WHERE rental_id = p_rental_id;
    
    -- Update available stock
    UPDATE Discs 
    SET available_stock = available_stock + 1
    WHERE disc_id = v_disc_id;
    
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Disc returned successfully.');
    
EXCEPTION
    WHEN v_rental_not_found THEN
        DBMS_OUTPUT.PUT_LINE('Error: Rental record not found.');
        ROLLBACK;
    WHEN v_already_returned THEN
        DBMS_OUTPUT.PUT_LINE('Error: This disc has already been returned.');
        ROLLBACK;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        ROLLBACK;
END ReturnDisc;
/

CREATE OR REPLACE FUNCTION GetActiveRentals(
    p_member_id IN NUMBER
) RETURN NUMBER AS
    v_count NUMBER;
    v_member_exists NUMBER;
BEGIN
    -- Check if member exists
    SELECT COUNT(*) INTO v_member_exists FROM Members WHERE member_id = p_member_id;
    IF v_member_exists = 0 THEN
        RETURN -1; -- Indicate invalid member ID
    END IF;
    
    -- Get count of active rentals
    SELECT COUNT(*) INTO v_count
    FROM Rentals
    WHERE member_id = p_member_id AND return_date IS NULL;
    
    RETURN v_count;
END GetActiveRentals;
/

CREATE OR REPLACE TRIGGER trg_prevent_zero_stock
BEFORE INSERT ON Rentals
FOR EACH ROW
DECLARE
    v_available NUMBER;
BEGIN
    -- Get available stock for the disc
    SELECT available_stock INTO v_available
    FROM Discs
    WHERE disc_id = :NEW.disc_id;
    
    -- Prevent rental if no stock available
    IF v_available <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Cannot rent disc: No stock available');
    END IF;
END;
/

-- Insert test members and additional mock data
BEGIN
    -- Test members
    INSERT INTO Members (member_id, name, phone_number, join_date)
    VALUES (member_seq.NEXTVAL, 'John Doe', '083999888', SYSDATE - 30);
    
    INSERT INTO Members (member_id, name, phone_number, join_date)
    VALUES (member_seq.NEXTVAL, 'Jane Smith', '082787989', SYSDATE - 15);
    
    INSERT INTO Members (member_id, name, phone_number, join_date)
    VALUES (member_seq.NEXTVAL, 'Alice Johnson', '0997896728', SYSDATE - 20);
    
    INSERT INTO Members (member_id, name, phone_number, join_date)
    VALUES (member_seq.NEXTVAL, 'Bob Brown', '0363777464', SYSDATE - 10);
    
    -- Test discs
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'Thriller', 'Michael Jackson', 'Pop', 3, 3);
    
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'Back in Black', 'AC/DC', 'Rock', 2, 2);
    
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'Kind of Blue', 'Miles Davis', 'Jazz', 1, 1);
    
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'Abbey Road', 'The Beatles', 'Rock', 4, 4);
    
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'The Dark Side of the Moon', 'Pink Floyd', 'Progressive Rock', 2, 2);
    
    INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock)
    VALUES (disc_seq.NEXTVAL, 'Rumours', 'Fleetwood Mac', 'Rock', 3, 3);
    
    COMMIT;
END;
/

SELECT * FROM Members;
SELECT * FROM Discs;
SELECT * FROM Rentals;

-- Test cases
-- 1: Rent a disc
BEGIN
    DBMS_OUTPUT.PUT_LINE('Test case 1: Rent a disc');
    RentDisc(1, 1); -- Member 1 rents Disc 1 (Thriller)
END;
/
-- 2: Try to rent a disc with no stock
BEGIN
    DBMS_OUTPUT.PUT_LINE('Test case 2: Rent all copies then try to rent again');
    RentDisc(1, 3); -- Member 1 rents Disc 3 (Kind of Blue)
    RentDisc(2, 3); -- Member 2 tries to rent Disc 3 (should fail)
END;
/
-- 3: Return a disc
BEGIN
    DBMS_OUTPUT.PUT_LINE('Test case 3: Return a disc');
    ReturnDisc(1); -- Return the first rental
    ReturnDisc(2);
END;
/
-- 4: Check active rentals
BEGIN
    DBMS_OUTPUT.PUT_LINE('Test case 4: Check active rentals for Member 1');
    DBMS_OUTPUT.PUT_LINE('Active rentals for Member 1: ' || GetActiveRentals(1));
END;
/

-- Clean up test data and reset sequences
TRUNCATE TABLE Rentals;
TRUNCATE TABLE Discs;
TRUNCATE TABLE Members;
DROP SEQUENCE rental_seq;
DROP SEQUENCE disc_seq;
DROP SEQUENCE member_seq;
CREATE SEQUENCE disc_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE member_seq START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE rental_seq START WITH 1 INCREMENT BY 1;

-- SQL Script to view all rentals
SELECT 
    m.member_id,
    m.name AS member_name,
    d.disc_id,
    d.title AS disc_title,
    d.artist,
    r.rental_date,
    r.return_date,
    CASE 
        WHEN r.return_date IS NULL THEN 'Currently Rented'
        ELSE 'Returned'
    END AS status
FROM 
    Rentals r
JOIN 
    Members m ON r.member_id = m.member_id
JOIN 
    Discs d ON r.disc_id = d.disc_id
ORDER BY 
    m.name, r.rental_date DESC;


UPDATE Discs
SET available_stock = available_stock + 1, 
    total_stock = total_stock + 1
WHERE disc_id = 1;
