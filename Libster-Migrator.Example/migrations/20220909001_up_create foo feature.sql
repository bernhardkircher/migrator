-- example script
create table dbo.Foo (
    id int identity primary key not null,
    something nvarchar(256) not null
);

insert into dbo.Foo (something) values ('this is a test');
