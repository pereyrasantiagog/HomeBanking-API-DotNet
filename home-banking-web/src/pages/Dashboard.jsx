import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';

const Dashboard = () => {
  const navigate = useNavigate();

  // Estados de la vista
  const [account, setAccount] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Función para obtener los datos al montar el componente
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        setError(null);

        // 1. Consultar el saldo con la ruta protegida que lee del token
        const accountRes = await api.get('/Accounts/MyAccount');
        const accountData = accountRes.data;

        // 2. Consultar el historial de transacciones con la nueva ruta sin ID
        const txRes = await api.get('/Transactions/History?pageNumber=1&pageSize=10');

        // Actualizamos los estados
        setAccount(accountData || null);

        // Dependiendo de cómo devuelva la paginación el backend, extraemos la data
        const txList = txRes.data.data || txRes.data.items || txRes.data;
        setTransactions(Array.isArray(txList) ? txList : []);

      } catch (err) {
        console.error('Error al cargar el dashboard:', err);
        setError('No pudimos cargar tu información. Por favor, intenta de nuevo más tarde.');
      } finally {
        setLoading(false); // Apagamos el estado de carga
      }
    };

    fetchDashboardData();
  }, []);

  // Función para cerrar sesión
  const handleLogout = () => {
    localStorage.removeItem('token'); // Borramos el JWT
    navigate('/login');               // Redirigimos al Login
  };

  // Función para manejar el depósito de dinero
  const handleDeposit = async () => {
    const amountStr = window.prompt('Ingrese el monto a depositar:');
    if (!amountStr) return;

    const amount = parseFloat(amountStr);
    if (isNaN(amount) || amount <= 0) {
      alert('Por favor, ingrese un monto válido mayor a cero.');
      return;
    }

    try {
      const res = await api.post('/Transactions/Deposit', { accountId: account.id, amount });
      
      if (res.status === 200) {
        // Actualizamos el saldo en el estado
        setAccount(prev => ({ ...prev, balance: res.data.newBalance }));
        
        // Agregamos la nueva transacción al inicio de la lista
        if (res.data.transaction) {
          setTransactions(prev => [res.data.transaction, ...prev]);
        }
        
        alert('Depósito realizado con éxito.');
      }
    } catch (err) {
      console.error('Error al depositar:', err);
      alert('Hubo un error al procesar el depósito. Inténtalo más tarde.');
    }
  };

  // Helper para formatear fechas
  const formatDate = (dateString) => {
    if (!dateString) return '-';
    const options = { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleDateString('es-ES', options);
  };

  // Helper para el tipo de transacción (según un posible Enum en .NET)
  const getTransactionLabel = (type) => {
    switch (type) {
      case 0: return 'Depósito';
      case 1: return 'Retiro';
      case 2: return 'Transferencia';
      default: return 'Movimiento';
    }
  };

  // Vista mientras carga
  if (loading) {
    return (
      <div className="dashboard-container loading">
        <p>Cargando tu información financiera...</p>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <h1>Mi Home Banking</h1>
        <button onClick={handleLogout} className="btn-secondary">Cerrar Sesión</button>
      </header>

      {error && <div className="alert-error">{error}</div>}

      <main className="dashboard-main">
        {/* Sección de la Cuenta y Saldo */}
        <section className="accounts-section">
          <h2>Mis Cuentas</h2>
          {account ? (
            <div className="card balance-card">
              <h3>Cuenta Corriente</h3>
              <p className="balance">
                $ {account.balance !== undefined ? account.balance.toFixed(2) : '0.00'}
              </p>
              <p className="account-number">
                Nro: {account.number || 'No asignado'}
              </p>
            </div>
          ) : (
            <p>No se encontró información de la cuenta.</p>
          )}
        </section>

        {/* Sección de Acciones Rápidas */}
        <section className="actions-section">
          <h2>Acciones Rápidas</h2>
          <div className="action-buttons">
            <button className="btn-primary" onClick={handleDeposit}>Depositar</button>
            <button className="btn-primary">Transferir</button>
            <button className="btn-primary">Pagar Servicios</button>
          </div>
        </section>

        {/* Sección del Historial de Transacciones */}
        <section className="transactions-section">
          <h2>Últimos Movimientos</h2>
          {transactions.length > 0 ? (
            <div className="table-responsive">
              <table className="transactions-table">
                <thead>
                  <tr>
                    <th>Fecha</th>
                    <th>Descripción</th>
                    <th>Tipo</th>
                    <th className="align-right">Monto</th>
                  </tr>
                </thead>
                <tbody>
                  {transactions.map((tx) => {
                    // Lógica simple para saber si suma o resta dinero
                    const isIncome = tx.type === 0 || tx.destinationAccountId === account?.id;
                    const amountClass = isIncome ? 'text-success' : 'text-danger';
                    const amountSign = isIncome ? '+' : '-';

                    return (
                      <tr key={tx.id}>
                        <td>{formatDate(tx.date)}</td>
                        <td>{tx.description || '-'}</td>
                        <td>{getTransactionLabel(tx.type)}</td>
                        <td className={`align-right ${amountClass}`}>
                          {amountSign} $ {tx.amount?.toFixed(2)}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="card empty-state">
              <p>No tienes movimientos recientes.</p>
            </div>
          )}
        </section>
      </main>
    </div>
  );
};

export default Dashboard;
