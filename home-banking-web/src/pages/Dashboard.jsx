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

  // ID estático temporal (tal como se solicitó)
  const TEMP_ACCOUNT_ID = 1;

  useEffect(() => {
    // Función para obtener los datos al montar el componente
    const fetchDashboardData = async () => {
      try {
        setLoading(true);
        setError(null);

        // 1. Consultar el saldo
        // Intentamos el endpoint 'MyAccount', si no existe aún, hacemos fallback al ID temporal
        let accountData = null;
        try {
          const accountRes = await api.get('/Accounts/MyAccount');
          accountData = accountRes.data;
        } catch (err) {
          // Fallback temporal si /Accounts/MyAccount no está implementado en el backend
          const accountFallbackRes = await api.get(`/Accounts/${TEMP_ACCOUNT_ID}`);
          accountData = accountFallbackRes.data;
        }

        // 2. Consultar el historial de transacciones con paginación
        const txRes = await api.get(`/Transactions/History/${TEMP_ACCOUNT_ID}?pageNumber=1&pageSize=10`);

        // Actualizamos los estados
        setAccount(accountData);

        // Dependiendo de cómo devuelva la paginación el backend, extraemos la data
        // Si el backend devuelve { data: [...], totalRecords: X }, usamos txRes.data.data
        // Si devuelve directamente el array, usamos txRes.data
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
                Nro: {account.number || `0000${TEMP_ACCOUNT_ID}`}
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
            <button className="btn-primary">Transferir</button>
            <button className="btn-primary">Pagar Servicios</button>
            <button className="btn-primary">Mis Tarjetas</button>
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
