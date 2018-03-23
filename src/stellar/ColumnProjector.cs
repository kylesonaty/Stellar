using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Stellar
{
    internal class ColumnProjector : DbExpressionVisitor
    {
        Nominator _nominator;
        Dictionary<ColumnExpression, ColumnExpression> _map;
        List<ColumnDeclaration> _columns;
        HashSet<string> _columnNames;
        HashSet<Expression> _candidates;
        string _existingAlias;
        string _newAlias;
        int _iColumn;

        internal ColumnProjector(Func<Expression, bool> fnCanBeColumn)
        {
            _nominator = new Nominator(fnCanBeColumn);
        }

        internal ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
        {
            _map = new Dictionary<ColumnExpression, ColumnExpression>();
            _columns = new List<ColumnDeclaration>();
            _columnNames = new HashSet<string>();
            _newAlias = newAlias;
            _existingAlias = existingAlias;
            _candidates = _nominator.Nominate(expression);
            return new ProjectedColumns(Visit(expression), _columns.AsReadOnly());
        }

        public override Expression Visit(Expression expression)
        {
            if (_candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Column)
                {
                    var column = (ColumnExpression)expression;
                    if (_map.TryGetValue(column, out ColumnExpression mapped))
                    {
                        return mapped;
                    }

                    if (_existingAlias == column.Alias)
                    {
                        var ordinal = _columns.Count;
                        var columnName = GetUniqueColumnName(column.Name);
                        _columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, _newAlias, columnName, ordinal);
                        _map[column] = mapped;
                        _columnNames.Add(columnName);
                        return mapped;
                    }
                    return column;
                }
                else
                {
                    var columnName = GetNextColumnName();
                    var ordinal = _columns.Count;
                    _columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, _newAlias, columnName, ordinal);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }


        private bool IsColumnNameIsUse(string name)
        {
            return _columnNames.Contains(name);
        }

        private string GetUniqueColumnName(string name)
        {
            var baseName = name;
            var suffix = 1;
            while (IsColumnNameIsUse(name))
            {
                name = baseName + (suffix++);
            }
            return name;
        }

        private string GetNextColumnName()
        {
            return GetUniqueColumnName("c" + (_iColumn++));
        }

        class Nominator : DbExpressionVisitor
        {
            Func<Expression, bool> _fnCanBeColumn;
            bool _isBlocked;
            HashSet<Expression> _candidates;

            internal Nominator(Func<Expression, bool> fnCanBeColumn)
            {
                _fnCanBeColumn = fnCanBeColumn;
            }

            internal HashSet<Expression> Nominate(Expression expression)
            {
                _candidates = new HashSet<Expression>();
                _isBlocked = false;
                Visit(expression);
                return _candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    var saveIsBlocked = _isBlocked;
                    _isBlocked = false;
                    base.Visit(expression);
                    if(!_isBlocked)
                    {
                        if (_fnCanBeColumn(expression))
                        {
                            _candidates.Add(expression);
                        }
                        else
                        {
                            _isBlocked = true;
                        }
                    }
                    _isBlocked |= saveIsBlocked;
                }
                return expression;
            }
        }
    }

    
}