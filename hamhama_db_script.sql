BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE ClientFeedback CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE Reservation CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE RestaurantTable CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE MenuItem CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE Client CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE Admin_tab CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP TABLE usr CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE seq_user';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE seq_menuitem';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE seq_table';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
  EXECUTE IMMEDIATE 'DROP SEQUENCE seq_reservation';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

CREATE SEQUENCE seq_user START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_menuitem START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_table START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_reservation START WITH 1 INCREMENT BY 1;

-- ????? ??????? ???? DEFAULT ??? NEXTVAL

CREATE TABLE usr (
    user_id INT PRIMARY KEY,
    full_name VARCHAR2(255) NOT NULL,
    email VARCHAR2(255),
    time_created DATE DEFAULT SYSDATE NOT NULL
);

CREATE TABLE Admin_tab (
    admin_id INT PRIMARY KEY,
    username VARCHAR2(100) NOT NULL,
    pwd VARCHAR2(100) NOT NULL,
    FOREIGN KEY (admin_id) REFERENCES usr(user_id),
    time_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Client (
    client_id INT PRIMARY KEY,
    full_name VARCHAR2(100) NOT NULL,
    num_phone VARCHAR2(20) NOT NULL,
    email VARCHAR2(100) NOT NULL,
    FOREIGN KEY (client_id) REFERENCES usr(user_id),
    time_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE MenuItem (
    item_id NUMBER PRIMARY KEY,
    name_item VARCHAR2(100) NOT NULL,
    disponible NUMBER(1) DEFAULT 1 CHECK (disponible IN (0, 1)),
    prix NUMBER(10, 2) NOT NULL,
    admin_id INT NOT NULL,
    CONSTRAINT fk_admin FOREIGN KEY (admin_id) REFERENCES Admin_tab(admin_id)
);

CREATE TABLE RestaurantTable (
    table_id NUMBER PRIMARY KEY,
    num_table NUMBER NOT NULL,
    seats NUMBER NOT NULL CHECK (seats BETWEEN 1 AND 10)
);

CREATE TABLE Reservation (
    reservation_id NUMBER PRIMARY KEY,
    reservation_datetime TIMESTAMP NOT NULL,
    nbr_personnes NUMBER NOT NULL,
    choix_item VARCHAR2(255),
    time_created TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    client_id INT,
    table_id INT,
    CONSTRAINT fk_client FOREIGN KEY (client_id) REFERENCES Client(client_id),
    CONSTRAINT fk_table FOREIGN KEY (table_id) REFERENCES RestaurantTable(table_id)
);

CREATE TABLE ClientFeedback (
    client_id INT NOT NULL,
    item_id INT NOT NULL,
    rating INT CHECK (rating BETWEEN 1 AND 5),
    comnt VARCHAR2(1000),
    date_interacted TIMESTAMP,
    CONSTRAINT fk_clientfeed FOREIGN KEY (client_id) REFERENCES Client(client_id),
    CONSTRAINT pk_clientfeed PRIMARY KEY (client_id, item_id)
);