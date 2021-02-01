--CREATE DATABASE Subchain;

DROP TABLE [dbo].[Users];
CREATE TABLE [dbo].[Users] (
    [UserId]          INT           IDENTITY (1, 1) NOT NULL,
    [CompanyName]     VARCHAR (50)  NOT NULL,
    [CompanyType]     VARCHAR (20)  NOT NULL,
    [Email]           VARCHAR (50)  NOT NULL,
    [Password]        VARCHAR (50)  NOT NULL,
    [Country]         VARCHAR (50)  NULL,
    [CityOrState]     VARCHAR (50)  NULL,
    [ProductCategory] VARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);

Select * from Users
DROP TABLE [dbo].[Product];
CREATE TABLE [dbo].[Product] (
    [ProductId]    INT          IDENTITY (1, 1) NOT NULL,
    [UserId]       INT          Not NULL,
    [ProductName]  VARCHAR (50) NOT NULL,
    [ProductDesc]  NCHAR (255)  NOT NULL,
    [ProductType]  VARCHAR (50) NOT NULL,
    [ProductPrice] SMALLMONEY   NOT NULL,
	[ImageUrl]     VARCHAR(MAX) NOT NULL, 
    PRIMARY KEY CLUSTERED ([ProductId] ASC)
);
CREATE TABLE [dbo].[Invoice] (
    [SupplierId]          INT           NOT NULL,
    [Date]     DATE  NOT NULL,
    PRIMARY KEY CLUSTERED ([SupplierId] ASC)
);



INSERT INTO Invoice
VALUES (1, '2020/11/11');

Select * from Invoice